using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ArasCatiaAddin.Models;

namespace ArasCatiaAddin
{
    /// <summary>
    /// Service class for CATIA V5 operations via COM Automation.
    /// Uses reflection-based invocation for compatibility with CATIA V5-6R2022.
    /// </summary>
    public class CatiaService : IDisposable
    {
        private object _catiaApp;
        private bool _disposed;

        public CatiaService(object catiaApplication)
        {
            _catiaApp = catiaApplication;
        }

        /// <summary>
        /// Get the active document in CATIA.
        /// </summary>
        public CatiaDocumentInfo GetActiveDocument(out string errorMessage)
        {
            errorMessage = null;

            if (_catiaApp == null)
            {
                errorMessage = "CATIA application not available.";
                return null;
            }

            try
            {
                // Get Documents collection
                object documents = GetProperty(_catiaApp, "Documents");
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
                    activeDoc = GetProperty(_catiaApp, "ActiveDocument");
                }
                catch
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
                    FileName = GetProperty(activeDoc, "Name")?.ToString(),
                    IsSaved = true
                };

                // Get Saved property
                try
                {
                    docInfo.IsSaved = Convert.ToBoolean(GetProperty(activeDoc, "Saved"));
                }
                catch { }

                // Determine document type
                string extension = Path.GetExtension(docInfo.FullPath).ToLower();
                docInfo.DocumentType = GetDocumentType(extension);

                // Extract properties based on type
                ExtractDocumentProperties(activeDoc, docInfo, extension);

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
        /// Extract properties from CATIA document.
        /// </summary>
        private void ExtractDocumentProperties(object doc, CatiaDocumentInfo docInfo, string extension)
        {
            try
            {
                if (extension == ".catpart")
                {
                    object part = GetProperty(doc, "Part");
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

                            try
                            {
                                object defParam = CallMethod(parameters, "Item", "Definition");
                                if (defParam != null)
                                {
                                    docInfo.Definition = GetProperty(defParam, "ValueAsString")?.ToString();
                                    Marshal.ReleaseComObject(defParam);
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
                    object product = GetProperty(doc, "Product");
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
            catch (Exception ex)
            {
                Logger.Debug($"ExtractDocumentProperties: {ex.Message}");
            }
        }

        /// <summary>
        /// Save the active document.
        /// </summary>
        public bool SaveActiveDocument(out string errorMessage)
        {
            errorMessage = null;

            if (_catiaApp == null)
            {
                errorMessage = "CATIA application not available.";
                return false;
            }

            try
            {
                object activeDoc = GetProperty(_catiaApp, "ActiveDocument");

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

            if (_catiaApp == null)
            {
                errorMessage = "CATIA application not available.";
                return false;
            }

            try
            {
                SetProperty(_catiaApp, "Visible", true);
                object documents = GetProperty(_catiaApp, "Documents");
                CallMethod(documents, "Open", filePath);
                Marshal.ReleaseComObject(documents);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error opening document: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Get BOM from a CATProduct assembly.
        /// </summary>
        public List<BomItem> GetBom(out string errorMessage)
        {
            errorMessage = null;
            var bomItems = new List<BomItem>();

            if (_catiaApp == null)
            {
                errorMessage = "CATIA application not available.";
                return bomItems;
            }

            try
            {
                object activeDoc = GetProperty(_catiaApp, "ActiveDocument");

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
        /// Recursively extract BOM items from assembly.
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
                            Quantity = 1,
                            SyncStatus = SyncStatus.New
                        };

                        try
                        {
                            object refProduct = GetProperty(child, "ReferenceProduct");
                            object parent = GetProperty(refProduct, "Parent");
                            bomItem.FilePath = GetProperty(parent, "FullName")?.ToString();
                            bomItem.FileType = Path.GetExtension(bomItem.FilePath)?.ToLower() == ".catpart"
                                ? "CATPart" : "CATProduct";
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
        /// Get document type from extension.
        /// </summary>
        private string GetDocumentType(string extension)
        {
            switch (extension)
            {
                case ".catpart": return "CATPart";
                case ".catproduct": return "CATProduct";
                case ".catdrawing": return "CATDrawing";
                case ".cgr": return "CGR";
                default: return "Unknown";
            }
        }

        #region COM Helper Methods

        private object GetProperty(object comObject, string propertyName)
        {
            return comObject.GetType().InvokeMember(propertyName,
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, null);
        }

        private void SetProperty(object comObject, string propertyName, object value)
        {
            comObject.GetType().InvokeMember(propertyName,
                BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, new object[] { value });
        }

        private object CallMethod(object comObject, string methodName, params object[] args)
        {
            return comObject.GetType().InvokeMember(methodName,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, args);
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _catiaApp = null;
                _disposed = true;
            }
        }
    }
}
