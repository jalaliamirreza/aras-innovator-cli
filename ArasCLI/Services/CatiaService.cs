using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace ArasCLI.Services
{
    /// <summary>
    /// Service class for CATIA V5 operations via COM Automation.
    /// </summary>
    public class CatiaService : IDisposable
    {
        private dynamic _catiaApp;
        private bool _isConnected;
        private bool _disposed;

        public bool IsConnected => _isConnected;

        /// <summary>
        /// Connect to a running CATIA instance.
        /// </summary>
        public bool Connect(out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // Try to get existing CATIA instance
                _catiaApp = Marshal.GetActiveObject("CATIA.Application");
                _isConnected = true;
                return true;
            }
            catch (COMException)
            {
                // CATIA is not running, try to start it
                try
                {
                    Type catiaType = Type.GetTypeFromProgID("CATIA.Application");
                    if (catiaType == null)
                    {
                        errorMessage = "CATIA is not installed or not properly registered.";
                        _isConnected = false;
                        return false;
                    }

                    _catiaApp = Activator.CreateInstance(catiaType);
                    _catiaApp.Visible = true;
                    _isConnected = true;
                    return true;
                }
                catch (Exception ex)
                {
                    errorMessage = "Could not start CATIA: " + ex.Message;
                    _isConnected = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Could not connect to CATIA: " + ex.Message;
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnect from CATIA (does not close CATIA).
        /// </summary>
        public void Disconnect()
        {
            if (_catiaApp != null)
            {
                try
                {
                    Marshal.ReleaseComObject(_catiaApp);
                }
                catch { }
                _catiaApp = null;
            }
            _isConnected = false;
        }

        /// <summary>
        /// Get the active document in CATIA.
        /// </summary>
        public CatiaDocumentInfo GetActiveDocument(out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _catiaApp == null)
            {
                errorMessage = "Not connected to CATIA.";
                return null;
            }

            try
            {
                dynamic activeDoc = _catiaApp.ActiveDocument;

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return null;
                }

                var docInfo = new CatiaDocumentInfo
                {
                    FullPath = activeDoc.FullName,
                    Name = activeDoc.Name,
                    IsSaved = !activeDoc.Saved // Saved property is inverted (true = has unsaved changes)
                };

                // Determine document type
                string extension = Path.GetExtension(docInfo.FullPath).ToLower();
                docInfo.DocumentType = GetDocumentType(extension);

                // Try to get part number if it's a Part or Product
                try
                {
                    if (extension == ".catpart")
                    {
                        dynamic part = activeDoc.Part;
                        docInfo.PartNumber = part.Parameters.Item("PartNumber").ValueAsString;

                        // Try to get other properties
                        try { docInfo.Nomenclature = part.Parameters.Item("Nomenclature").ValueAsString; } catch { }
                        try { docInfo.Revision = part.Parameters.Item("Revision").ValueAsString; } catch { }
                        try { docInfo.Definition = part.Parameters.Item("Definition").ValueAsString; } catch { }
                    }
                    else if (extension == ".catproduct")
                    {
                        dynamic product = activeDoc.Product;
                        docInfo.PartNumber = product.PartNumber;
                        docInfo.Nomenclature = product.Nomenclature;
                        docInfo.Revision = product.Revision;
                        docInfo.Definition = product.Definition;
                    }
                }
                catch { }

                return docInfo;
            }
            catch (Exception ex)
            {
                errorMessage = "Error getting active document: " + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Save the active document.
        /// </summary>
        public bool SaveActiveDocument(out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _catiaApp == null)
            {
                errorMessage = "Not connected to CATIA.";
                return false;
            }

            try
            {
                dynamic activeDoc = _catiaApp.ActiveDocument;

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return false;
                }

                activeDoc.Save();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error saving document: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Save the active document to a specific path.
        /// </summary>
        public bool SaveActiveDocumentAs(string filePath, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _catiaApp == null)
            {
                errorMessage = "Not connected to CATIA.";
                return false;
            }

            try
            {
                dynamic activeDoc = _catiaApp.ActiveDocument;

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return false;
                }

                activeDoc.SaveAs(filePath);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error saving document: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Open a document in CATIA.
        /// </summary>
        public bool OpenDocument(string filePath, out string errorMessage)
        {
            errorMessage = null;

            if (!File.Exists(filePath))
            {
                errorMessage = "File not found: " + filePath;
                return false;
            }

            // First, try to connect to running CATIA
            if (!_isConnected || _catiaApp == null)
            {
                Connect(out _); // Try to connect, ignore error
            }

            // If connected to CATIA, use COM to open document
            if (_isConnected && _catiaApp != null)
            {
                try
                {
                    _catiaApp.Visible = true;
                    dynamic documents = _catiaApp.Documents;
                    documents.Open(filePath);

                    // Bring CATIA window to front
                    try { _catiaApp.Windows.Item(1).Activate(); } catch { }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("CATIA COM Open failed: " + ex.Message);
                    // Fall through to shell method
                }
            }

            // Fall back to Windows Shell
            return OpenWithShell(filePath, out errorMessage);
        }

        /// <summary>
        /// Open a file using Windows Shell (default application).
        /// </summary>
        private bool OpenWithShell(string filePath, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                // CATIA V5-6R2022 (B32) uses CATSTART.exe with environment parameters
                string catStartPath = @"C:\Program Files\Dassault Systemes\B32\win_b64\code\bin\CATSTART.exe";

                var process = new System.Diagnostics.Process();

                if (File.Exists(catStartPath))
                {
                    // Launch CATIA using CATSTART with proper environment
                    process.StartInfo.FileName = catStartPath;
                    process.StartInfo.Arguments = $"-run \"CNEXT.exe\" -env CATIA_P3.V5-6R2022.B32 -direnv \"C:\\ProgramData\\DassaultSystemes\\CATEnv\" -nowindow -object \"{filePath}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    errorMessage = "File sent to CATIA via CATSTART. Please check CATIA window.";
                    return true;
                }
                else
                {
                    // Fall back to shell execute (relies on file association)
                    process.StartInfo.FileName = filePath;
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                    errorMessage = "File opened with default application. Please check CATIA window.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Could not open file: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Close the active document.
        /// </summary>
        public bool CloseActiveDocument(bool save, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _catiaApp == null)
            {
                errorMessage = "Not connected to CATIA.";
                return false;
            }

            try
            {
                dynamic activeDoc = _catiaApp.ActiveDocument;

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return false;
                }

                if (save)
                {
                    activeDoc.Save();
                }

                activeDoc.Close();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error closing document: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Get BOM (Bill of Materials) from a CATProduct assembly.
        /// </summary>
        public List<BomItem> GetBom(out string errorMessage)
        {
            errorMessage = null;
            var bomItems = new List<BomItem>();

            if (!_isConnected || _catiaApp == null)
            {
                errorMessage = "Not connected to CATIA.";
                return bomItems;
            }

            try
            {
                dynamic activeDoc = _catiaApp.ActiveDocument;

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return bomItems;
                }

                string extension = Path.GetExtension((string)activeDoc.FullName).ToLower();
                if (extension != ".catproduct")
                {
                    errorMessage = "Active document is not a CATProduct assembly.";
                    return bomItems;
                }

                dynamic rootProduct = activeDoc.Product;
                ExtractBomRecursive(rootProduct, bomItems, 0);

                return bomItems;
            }
            catch (Exception ex)
            {
                errorMessage = "Error extracting BOM: " + ex.Message;
                return bomItems;
            }
        }

        /// <summary>
        /// Recursively extract BOM items from assembly.
        /// </summary>
        private void ExtractBomRecursive(dynamic product, List<BomItem> bomItems, int level)
        {
            try
            {
                dynamic children = product.Products;
                int childCount = children.Count;

                for (int i = 1; i <= childCount; i++)
                {
                    try
                    {
                        dynamic child = children.Item(i);

                        var bomItem = new BomItem
                        {
                            PartNumber = child.PartNumber,
                            Name = child.Name,
                            Nomenclature = child.Nomenclature,
                            Revision = child.Revision,
                            Level = level,
                            Quantity = 1 // Would need to count occurrences for accurate quantity
                        };

                        try
                        {
                            bomItem.FilePath = child.ReferenceProduct.Parent.FullName;
                        }
                        catch { }

                        bomItems.Add(bomItem);

                        // Recurse into sub-assemblies
                        try
                        {
                            dynamic subProducts = child.Products;
                            if (subProducts != null && subProducts.Count > 0)
                            {
                                ExtractBomRecursive(child, bomItems, level + 1);
                            }
                        }
                        catch { }
                    }
                    catch { }
                }
            }
            catch { }
        }

        /// <summary>
        /// Get document type from file extension.
        /// </summary>
        private string GetDocumentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".catpart":
                    return "Part";
                case ".catproduct":
                    return "Product";
                case ".catdrawing":
                    return "Drawing";
                case ".cgr":
                    return "CGR";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Check if CATIA is running.
        /// </summary>
        public static bool IsCatiaRunning()
        {
            try
            {
                dynamic catia = Marshal.GetActiveObject("CATIA.Application");
                Marshal.ReleaseComObject(catia);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Information about a CATIA document.
    /// </summary>
    public class CatiaDocumentInfo
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public string DocumentType { get; set; }
        public string PartNumber { get; set; }
        public string Nomenclature { get; set; }
        public string Revision { get; set; }
        public string Definition { get; set; }
        public bool IsSaved { get; set; }
    }

    /// <summary>
    /// BOM item from CATIA assembly.
    /// </summary>
    public class BomItem
    {
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public string Nomenclature { get; set; }
        public string Revision { get; set; }
        public string FilePath { get; set; }
        public int Level { get; set; }
        public int Quantity { get; set; }
    }
}
