using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;

namespace ArasCLI.Services
{
    /// <summary>
    /// Service class for CATIA V5 operations via COM Automation.
    /// Uses reflection-based invocation for better compatibility with CATIA V5-6R2022.
    /// </summary>
    public class CatiaService : IDisposable
    {
        private object _catiaApp;
        private Type _catiaType;
        private bool _isConnected;
        private bool _disposed;

        public bool IsConnected => _isConnected;

        /// <summary>
        /// Connect to CATIA instance using reflection-based approach.
        /// </summary>
        public bool Connect(out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // Get CATIA COM type from registry
                _catiaType = Type.GetTypeFromProgID("CATIA.Application");

                if (_catiaType == null)
                {
                    errorMessage = "CATIA is not installed or not registered on this system.";
                    return false;
                }

                // Try to connect to existing CATIA instance first
                try
                {
                    _catiaApp = Marshal.GetActiveObject("CATIA.Application");
                    if (_catiaApp != null)
                    {
                        _isConnected = true;
                        return true;
                    }
                }
                catch
                {
                    // No running instance, create new one
                }

                // Create new CATIA instance
                _catiaApp = Activator.CreateInstance(_catiaType);

                if (_catiaApp == null)
                {
                    errorMessage = "Failed to create CATIA instance.";
                    return false;
                }

                // Wait for CATIA to initialize
                System.Threading.Thread.Sleep(1000);

                // Make CATIA visible
                try
                {
                    InvokeSet("Visible", true);
                }
                catch { }

                _isConnected = true;
                return true;
            }
            catch (COMException ex)
            {
                errorMessage = $"COM Error connecting to CATIA: {ex.Message}";
                _isConnected = false;
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error connecting to CATIA: {ex.Message}";
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Connect to an existing running CATIA instance only (don't create new).
        /// </summary>
        public bool ConnectToRunning(out string errorMessage)
        {
            errorMessage = null;

            try
            {
                _catiaType = Type.GetTypeFromProgID("CATIA.Application");

                if (_catiaType == null)
                {
                    errorMessage = "CATIA is not installed or not registered on this system.";
                    return false;
                }

                // Only connect to existing instance
                _catiaApp = Marshal.GetActiveObject("CATIA.Application");

                if (_catiaApp == null)
                {
                    errorMessage = "CATIA is not running.";
                    return false;
                }

                _isConnected = true;
                return true;
            }
            catch (COMException)
            {
                errorMessage = "CATIA is not running. Please start CATIA first.";
                _isConnected = false;
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error connecting to CATIA: {ex.Message}";
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Invoke a method on the CATIA COM object using reflection.
        /// </summary>
        private object InvokeMethod(string methodName, params object[] args)
        {
            return _catiaType.InvokeMember(methodName,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, _catiaApp, args);
        }

        /// <summary>
        /// Get a property from the CATIA COM object using reflection.
        /// </summary>
        private object InvokeGet(string propertyName)
        {
            return _catiaType.InvokeMember(propertyName,
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, _catiaApp, null);
        }

        /// <summary>
        /// Set a property on the CATIA COM object using reflection.
        /// </summary>
        private void InvokeSet(string propertyName, object value)
        {
            _catiaType.InvokeMember(propertyName,
                BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, _catiaApp, new object[] { value });
        }

        /// <summary>
        /// Get a property from a COM object using reflection.
        /// </summary>
        private object GetProperty(object comObject, string propertyName)
        {
            return comObject.GetType().InvokeMember(propertyName,
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, null);
        }

        /// <summary>
        /// Set a property on a COM object using reflection.
        /// </summary>
        private void SetProperty(object comObject, string propertyName, object value)
        {
            comObject.GetType().InvokeMember(propertyName,
                BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, new object[] { value });
        }

        /// <summary>
        /// Invoke a method on a COM object using reflection.
        /// </summary>
        private object CallMethod(object comObject, string methodName, params object[] args)
        {
            return comObject.GetType().InvokeMember(methodName,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, args);
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
        /// Get the active document in CATIA using reflection.
        /// </summary>
        public CatiaDocumentInfo GetActiveDocument(out string errorMessage)
        {
            errorMessage = null;

            // Try to connect if not connected
            if (!_isConnected || _catiaApp == null)
            {
                if (!Connect(out string connectError))
                {
                    errorMessage = "Could not connect to CATIA: " + connectError;
                    return null;
                }
            }

            try
            {
                // Get Documents collection using reflection
                object documents = InvokeGet("Documents");
                if (documents == null)
                {
                    errorMessage = "Could not access CATIA Documents collection.";
                    return null;
                }

                int docCount = Convert.ToInt32(GetProperty(documents, "Count"));

                if (docCount == 0)
                {
                    errorMessage = "No document is open in CATIA.";
                    Marshal.ReleaseComObject(documents);
                    return null;
                }

                object activeDoc = null;
                try
                {
                    activeDoc = InvokeGet("ActiveDocument");
                }
                catch (Exception)
                {
                    // If ActiveDocument fails, try to get the first document
                    activeDoc = CallMethod(documents, "Item", 1);
                }

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    Marshal.ReleaseComObject(documents);
                    return null;
                }

                var docInfo = new CatiaDocumentInfo
                {
                    FullPath = GetProperty(activeDoc, "FullName")?.ToString(),
                    Name = GetProperty(activeDoc, "Name")?.ToString(),
                    IsSaved = true // Default to saved
                };

                // Try to get Saved property safely
                try
                {
                    bool saved = Convert.ToBoolean(GetProperty(activeDoc, "Saved"));
                    docInfo.IsSaved = saved;
                }
                catch { }

                // Determine document type
                string extension = Path.GetExtension(docInfo.FullPath).ToLower();
                docInfo.DocumentType = GetDocumentType(extension);

                // Try to get part number if it's a Part or Product
                try
                {
                    if (extension == ".catpart")
                    {
                        object part = GetProperty(activeDoc, "Part");
                        if (part != null)
                        {
                            object parameters = GetProperty(part, "Parameters");
                            if (parameters != null)
                            {
                                try
                                {
                                    object partNumParam = CallMethod(parameters, "Item", "PartNumber");
                                    if (partNumParam != null)
                                    {
                                        docInfo.PartNumber = GetProperty(partNumParam, "ValueAsString")?.ToString();
                                        Marshal.ReleaseComObject(partNumParam);
                                    }
                                }
                                catch { }

                                try
                                {
                                    object nomParam = CallMethod(parameters, "Item", "Nomenclature");
                                    if (nomParam != null)
                                    {
                                        docInfo.Nomenclature = GetProperty(nomParam, "ValueAsString")?.ToString();
                                        Marshal.ReleaseComObject(nomParam);
                                    }
                                }
                                catch { }

                                Marshal.ReleaseComObject(parameters);
                            }
                            Marshal.ReleaseComObject(part);
                        }
                    }
                    else if (extension == ".catproduct")
                    {
                        object product = GetProperty(activeDoc, "Product");
                        if (product != null)
                        {
                            docInfo.PartNumber = GetProperty(product, "PartNumber")?.ToString();
                            docInfo.Nomenclature = GetProperty(product, "Nomenclature")?.ToString();
                            docInfo.Revision = GetProperty(product, "Revision")?.ToString();
                            docInfo.Definition = GetProperty(product, "Definition")?.ToString();
                            Marshal.ReleaseComObject(product);
                        }
                    }
                }
                catch { }

                Marshal.ReleaseComObject(activeDoc);
                Marshal.ReleaseComObject(documents);

                return docInfo;
            }
            catch (Exception ex)
            {
                errorMessage = "Error getting active document: " + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Save the active document using reflection.
        /// </summary>
        public bool SaveActiveDocument(out string errorMessage)
        {
            errorMessage = null;

            // Try to connect if not connected
            if (!_isConnected || _catiaApp == null)
            {
                if (!Connect(out string connectError))
                {
                    errorMessage = "Could not connect to CATIA: " + connectError;
                    return false;
                }
            }

            try
            {
                object activeDoc = InvokeGet("ActiveDocument");

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return false;
                }

                CallMethod(activeDoc, "Save");
                Marshal.ReleaseComObject(activeDoc);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error saving document: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Save the active document to a specific path using reflection.
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
                object activeDoc = InvokeGet("ActiveDocument");

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return false;
                }

                CallMethod(activeDoc, "SaveAs", filePath);
                Marshal.ReleaseComObject(activeDoc);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error saving document: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Open a document in CATIA using reflection.
        /// </summary>
        public bool OpenDocument(string filePath, out string errorMessage)
        {
            errorMessage = null;

            if (!File.Exists(filePath))
            {
                errorMessage = "File not found: " + filePath;
                return false;
            }

            // First, try to connect to CATIA
            if (!_isConnected || _catiaApp == null)
            {
                Connect(out _); // Try to connect, ignore error
            }

            // If connected to CATIA, use COM to open document
            if (_isConnected && _catiaApp != null)
            {
                try
                {
                    InvokeSet("Visible", true);
                    object documents = InvokeGet("Documents");
                    CallMethod(documents, "Open", filePath);

                    // Bring CATIA window to front
                    try
                    {
                        object windows = InvokeGet("Windows");
                        object window = CallMethod(windows, "Item", 1);
                        CallMethod(window, "Activate");
                        Marshal.ReleaseComObject(window);
                        Marshal.ReleaseComObject(windows);
                    }
                    catch { }

                    Marshal.ReleaseComObject(documents);
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
        /// Close the active document using reflection.
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
                object activeDoc = InvokeGet("ActiveDocument");

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return false;
                }

                if (save)
                {
                    CallMethod(activeDoc, "Save");
                }

                CallMethod(activeDoc, "Close");
                Marshal.ReleaseComObject(activeDoc);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error closing document: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Get BOM (Bill of Materials) from a CATProduct assembly using reflection.
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
                object activeDoc = InvokeGet("ActiveDocument");

                if (activeDoc == null)
                {
                    errorMessage = "No document is open in CATIA.";
                    return bomItems;
                }

                string fullName = GetProperty(activeDoc, "FullName")?.ToString();
                string extension = Path.GetExtension(fullName).ToLower();
                if (extension != ".catproduct")
                {
                    errorMessage = "Active document is not a CATProduct assembly.";
                    Marshal.ReleaseComObject(activeDoc);
                    return bomItems;
                }

                object rootProduct = GetProperty(activeDoc, "Product");
                ExtractBomRecursive(rootProduct, bomItems, 0);

                Marshal.ReleaseComObject(rootProduct);
                Marshal.ReleaseComObject(activeDoc);

                return bomItems;
            }
            catch (Exception ex)
            {
                errorMessage = "Error extracting BOM: " + ex.Message;
                return bomItems;
            }
        }

        /// <summary>
        /// Recursively extract BOM items from assembly using reflection.
        /// </summary>
        private void ExtractBomRecursive(object product, List<BomItem> bomItems, int level)
        {
            try
            {
                object children = GetProperty(product, "Products");
                int childCount = Convert.ToInt32(GetProperty(children, "Count"));

                for (int i = 1; i <= childCount; i++)
                {
                    try
                    {
                        object child = CallMethod(children, "Item", i);

                        var bomItem = new BomItem
                        {
                            PartNumber = GetProperty(child, "PartNumber")?.ToString(),
                            Name = GetProperty(child, "Name")?.ToString(),
                            Nomenclature = GetProperty(child, "Nomenclature")?.ToString(),
                            Revision = GetProperty(child, "Revision")?.ToString(),
                            Level = level,
                            Quantity = 1 // Would need to count occurrences for accurate quantity
                        };

                        try
                        {
                            object refProduct = GetProperty(child, "ReferenceProduct");
                            object parent = GetProperty(refProduct, "Parent");
                            bomItem.FilePath = GetProperty(parent, "FullName")?.ToString();
                            Marshal.ReleaseComObject(parent);
                            Marshal.ReleaseComObject(refProduct);
                        }
                        catch { }

                        bomItems.Add(bomItem);

                        // Recurse into sub-assemblies
                        try
                        {
                            object subProducts = GetProperty(child, "Products");
                            if (subProducts != null)
                            {
                                int subCount = Convert.ToInt32(GetProperty(subProducts, "Count"));
                                if (subCount > 0)
                                {
                                    ExtractBomRecursive(child, bomItems, level + 1);
                                }
                                Marshal.ReleaseComObject(subProducts);
                            }
                        }
                        catch { }

                        Marshal.ReleaseComObject(child);
                    }
                    catch { }
                }

                Marshal.ReleaseComObject(children);
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
        /// Check if CATIA is running by looking for the process.
        /// </summary>
        public static bool IsCatiaRunning()
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName("CNEXT");
                return processes.Length > 0;
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
