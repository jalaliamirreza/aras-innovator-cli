using System;
using System.IO;
using Aras.IOM;

namespace ArasCLI.Services
{
    /// <summary>
    /// Service class for Aras Innovator operations including connection, document management, and file operations.
    /// </summary>
    public class ArasService : IDisposable
    {
        private HttpServerConnection _connection;
        private Innovator _innovator;
        private bool _isConnected;
        private bool _disposed;

        public bool IsConnected => _isConnected;
        public Innovator Innovator => _innovator;
        public HttpServerConnection Connection => _connection;

        /// <summary>
        /// Connect and login to Aras Innovator server.
        /// </summary>
        public bool Connect(string url, string database, string username, string password, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                _connection = IomFactory.CreateHttpServerConnection(url, database, username, password);
                Item loginResult = _connection.Login();

                if (loginResult.isError())
                {
                    errorMessage = loginResult.getErrorString();
                    _isConnected = false;
                    return false;
                }

                _innovator = IomFactory.CreateInnovator(_connection);
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnect from Aras Innovator server.
        /// </summary>
        public void Disconnect()
        {
            if (_connection != null && _isConnected)
            {
                try
                {
                    _connection.Logout();
                }
                catch { }
                _isConnected = false;
            }
        }

        /// <summary>
        /// Create a document and upload a file to Aras vault.
        /// </summary>
        public Item CreateDocumentWithFile(string filePath, string itemNumber, string documentName, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _innovator == null)
            {
                errorMessage = "Not connected to Aras server.";
                return null;
            }

            try
            {
                // Create Document item
                Item document = _innovator.newItem("Document", "add");

                // Set item_number
                if (!string.IsNullOrEmpty(itemNumber))
                {
                    document.setProperty("item_number", itemNumber);
                }
                else
                {
                    string generatedItemNumber = "DOC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    document.setProperty("item_number", generatedItemNumber);
                }

                // Set name
                if (!string.IsNullOrEmpty(documentName))
                {
                    document.setProperty("name", documentName);
                }

                // Apply document
                document = document.apply();

                if (document.isError())
                {
                    errorMessage = "Error creating document: " + document.getErrorString();
                    return document;
                }

                // Upload file
                string fileName = Path.GetFileName(filePath);
                string absoluteFilePath = Path.GetFullPath(filePath);

                Item fileItem = _innovator.newItem("File", "add");
                fileItem.setProperty("filename", fileName);
                fileItem.attachPhysicalFile(absoluteFilePath);

                Item fileResult = fileItem.apply();

                if (fileResult.isError())
                {
                    errorMessage = "File upload error: " + fileResult.getErrorString();
                    return document; // Return document even if file upload fails
                }

                // Create Document File relationship
                Item docFileRel = document.createRelationship("Document File", "add");
                docFileRel.setRelatedItem(fileResult);
                Item relResult = docFileRel.apply();

                if (relResult.isError())
                {
                    // Try alternative method
                    Item altRel = _innovator.newItem("Document File", "add");
                    altRel.setProperty("source_id", document.getID());
                    altRel.setProperty("related_id", fileResult.getID());
                    altRel.apply();
                }

                return document;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return _innovator.newError(ex.Message);
            }
        }

        /// <summary>
        /// Lock (check-out) an item in Aras.
        /// </summary>
        public Item LockItem(string itemType, string itemId, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _innovator == null)
            {
                errorMessage = "Not connected to Aras server.";
                return null;
            }

            try
            {
                Item item = _innovator.newItem(itemType, "lock");
                item.setID(itemId);
                Item result = item.apply();

                if (result.isError())
                {
                    errorMessage = result.getErrorString();
                }

                return result;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return _innovator.newError(ex.Message);
            }
        }

        /// <summary>
        /// Unlock (check-in) an item in Aras.
        /// </summary>
        public Item UnlockItem(string itemType, string itemId, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _innovator == null)
            {
                errorMessage = "Not connected to Aras server.";
                return null;
            }

            try
            {
                Item item = _innovator.newItem(itemType, "unlock");
                item.setID(itemId);
                Item result = item.apply();

                if (result.isError())
                {
                    errorMessage = result.getErrorString();
                }

                return result;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return _innovator.newError(ex.Message);
            }
        }

        /// <summary>
        /// Search for documents in Aras.
        /// </summary>
        public Item SearchDocuments(string searchCriteria, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _innovator == null)
            {
                errorMessage = "Not connected to Aras server.";
                return null;
            }

            try
            {
                Item search = _innovator.newItem("Document", "get");
                search.setAttribute("select", "id,item_number,name,state,locked_by_id,created_on,modified_on");

                if (!string.IsNullOrEmpty(searchCriteria))
                {
                    // Search by item_number or name using wildcard
                    search.setProperty("item_number", "*" + searchCriteria + "*");
                    search.setPropertyCondition("item_number", "like");
                }

                Item result = search.apply();

                if (result.isError())
                {
                    errorMessage = result.getErrorString();
                }

                return result;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return _innovator.newError(ex.Message);
            }
        }

        /// <summary>
        /// Get a document by ID with its files.
        /// </summary>
        public Item GetDocumentWithFiles(string documentId, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _innovator == null)
            {
                errorMessage = "Not connected to Aras server.";
                return null;
            }

            try
            {
                Item docQuery = _innovator.newItem("Document", "get");
                docQuery.setID(documentId);
                docQuery.setAttribute("select", "id,item_number,name,state,locked_by_id");

                Item docFileRel = docQuery.createRelationship("Document File", "get");
                docFileRel.setAttribute("select", "id,related_id");

                Item fileQuery = docFileRel.createRelatedItem("File", "get");
                fileQuery.setAttribute("select", "id,filename,file_size,checksum");

                Item result = docQuery.apply();

                if (result.isError())
                {
                    errorMessage = result.getErrorString();
                }

                return result;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return _innovator.newError(ex.Message);
            }
        }

        /// <summary>
        /// Download a file from Aras vault to local path.
        /// </summary>
        public bool DownloadFile(string fileId, string localPath, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _innovator == null)
            {
                errorMessage = "Not connected to Aras server.";
                return false;
            }

            try
            {
                // Get the file item
                Item fileItem = _innovator.getItemById("File", fileId);

                if (fileItem == null || fileItem.isError())
                {
                    errorMessage = "File not found: " + fileId;
                    return false;
                }

                // Get file URL from vault
                string fileUrl = _connection.getFileUrl(fileId, UrlType.SecurityToken);

                if (string.IsNullOrEmpty(fileUrl))
                {
                    errorMessage = "Could not get file URL from vault.";
                    return false;
                }

                // Download file using HttpClient
                using (var client = new System.Net.WebClient())
                {
                    client.DownloadFile(fileUrl, localPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Update an existing document with a new file version.
        /// </summary>
        public Item UpdateDocumentFile(string documentId, string filePath, out string errorMessage)
        {
            errorMessage = null;

            if (!_isConnected || _innovator == null)
            {
                errorMessage = "Not connected to Aras server.";
                return null;
            }

            try
            {
                // First, lock the document
                Item lockResult = LockItem("Document", documentId, out string lockError);
                if (lockResult == null || lockResult.isError())
                {
                    errorMessage = "Could not lock document: " + lockError;
                    return lockResult;
                }

                // Upload new file
                string fileName = Path.GetFileName(filePath);
                string absoluteFilePath = Path.GetFullPath(filePath);

                Item fileItem = _innovator.newItem("File", "add");
                fileItem.setProperty("filename", fileName);
                fileItem.attachPhysicalFile(absoluteFilePath);

                Item fileResult = fileItem.apply();

                if (fileResult.isError())
                {
                    // Unlock document on failure
                    UnlockItem("Document", documentId, out _);
                    errorMessage = "File upload error: " + fileResult.getErrorString();
                    return fileResult;
                }

                // Create new Document File relationship
                Item document = _innovator.getItemById("Document", documentId);
                Item docFileRel = document.createRelationship("Document File", "add");
                docFileRel.setRelatedItem(fileResult);
                Item relResult = docFileRel.apply();

                // Unlock the document
                UnlockItem("Document", documentId, out _);

                if (relResult.isError())
                {
                    errorMessage = "Could not create relationship: " + relResult.getErrorString();
                }

                return document;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return _innovator.newError(ex.Message);
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
}
