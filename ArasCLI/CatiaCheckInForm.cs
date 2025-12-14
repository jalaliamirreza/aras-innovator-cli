using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Aras.IOM;
using ArasCLI.Services;

namespace ArasCLI
{
    public class CatiaCheckInForm : Form
    {
        // Controls
        private Label lblTitle;
        private Label lblInfo;
        private ListView lvCheckedOut;
        private Button btnRefresh;
        private Button btnCheckIn;
        private Button btnCancel;
        private Label lblStatus;
        private Panel statusPanel;
        private ProgressBar progressBar;
        private CheckBox chkCloseInCatia;

        // Services
        private ConfigManager _configManager;

        public CatiaCheckInForm()
        {
            _configManager = new ConfigManager();
            InitializeComponents();
            CheckSession();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Aras Innovator - Check In";
            this.Size = new Size(700, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            int yPos = 20;
            int leftMargin = 20;
            int rightMargin = 20;
            int formWidth = this.ClientSize.Width - leftMargin - rightMargin;

            // Title
            lblTitle = new Label
            {
                Text = "Check In to Aras",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(leftMargin, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);
            yPos += 40;

            // Info
            lblInfo = new Label
            {
                Text = "Items locked by you (ready for check-in):",
                Location = new Point(leftMargin, yPos),
                Size = new Size(formWidth - 100, 20)
            };
            this.Controls.Add(lblInfo);

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(leftMargin + formWidth - 80, yPos - 5),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);
            yPos += 30;

            // Results ListView
            lvCheckedOut = new ListView
            {
                Location = new Point(leftMargin, yPos),
                Size = new Size(formWidth, 220),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                CheckBoxes = true
            };
            lvCheckedOut.Columns.Add("Item Number", 100);
            lvCheckedOut.Columns.Add("Name", 130);
            lvCheckedOut.Columns.Add("Rev", 40);
            lvCheckedOut.Columns.Add("State", 80);
            lvCheckedOut.Columns.Add("Type", 70);
            lvCheckedOut.Columns.Add("Local File", 180);
            lvCheckedOut.ItemChecked += LvCheckedOut_ItemChecked;

            this.Controls.Add(lvCheckedOut);
            yPos += 230;

            // Close in CATIA checkbox
            chkCloseInCatia = new CheckBox
            {
                Text = "Close document in CATIA after check-in",
                Location = new Point(leftMargin, yPos),
                Size = new Size(300, 25),
                Checked = false
            };
            this.Controls.Add(chkCloseInCatia);
            yPos += 35;

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(leftMargin, yPos),
                Size = new Size(formWidth, 20),
                Visible = false
            };
            this.Controls.Add(progressBar);
            yPos += 30;

            // Buttons
            btnCheckIn = new Button
            {
                Text = "Check In",
                Location = new Point(leftMargin + formWidth - 220, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnCheckIn.FlatAppearance.BorderSize = 0;
            btnCheckIn.Click += BtnCheckIn_Click;

            btnCancel = new Button
            {
                Text = "Close",
                Location = new Point(leftMargin + formWidth - 110, yPos),
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.Add(btnCheckIn);
            this.Controls.Add(btnCancel);
            yPos += 50;

            // Status panel
            statusPanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(700, 40),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(leftMargin, 10),
                Size = new Size(650, 20),
                ForeColor = Color.Gray
            };
            statusPanel.Controls.Add(lblStatus);
            this.Controls.Add(statusPanel);

            // Cancel button
            this.CancelButton = btnCancel;
        }

        private void CheckSession()
        {
            if (!SessionManager.HasActiveSession)
            {
                SetStatus("Not logged in. Please login first.", Color.Red);
                btnRefresh.Enabled = false;

                var result = MessageBox.Show(
                    "You are not logged in to Aras Innovator.\n\nWould you like to login now?",
                    "Login Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using (var loginForm = new CatiaLoginForm())
                    {
                        loginForm.ShowDialog();
                        if (SessionManager.HasActiveSession)
                        {
                            SetStatus($"Connected as: {SessionManager.Username}", Color.Green);
                            btnRefresh.Enabled = true;
                            LoadCheckedOutItems();
                        }
                    }
                }
            }
            else
            {
                SetStatus($"Connected as: {SessionManager.Username}", Color.Green);
                LoadCheckedOutItems();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadCheckedOutItems();
        }

        private void LoadCheckedOutItems()
        {
            if (!SessionManager.HasActiveSession)
            {
                MessageBox.Show("Please login first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus("Loading locked items...", Color.Orange);
            lvCheckedOut.Items.Clear();
            btnCheckIn.Enabled = false;
            Application.DoEvents();

            try
            {
                var innovator = SessionManager.Innovator;
                string currentUserId = innovator.getUserID();
                string workspace = _configManager.Config.LocalWorkspace;

                // Search for CAD items locked by current user
                Item query = innovator.newItem("CAD", "get");
                query.setAttribute("select", "id,item_number,name,major_rev,state,locked_by_id,native_file");
                query.setProperty("locked_by_id", currentUserId);
                query.setAttribute("orderBy", "modified_on DESC");
                query.setAttribute("maxRecords", "50");

                Item result = query.apply();

                if (!result.isError())
                {
                    int count = result.getItemCount();
                    for (int i = 0; i < count; i++)
                    {
                        Item item = result.getItemByIndex(i);
                        AddItemToList(item, "CAD", workspace, innovator);
                    }
                }

                // Also search for Document items locked by current user
                query = innovator.newItem("Document", "get");
                query.setAttribute("select", "id,item_number,name,major_rev,state,locked_by_id");
                query.setProperty("locked_by_id", currentUserId);
                query.setAttribute("orderBy", "modified_on DESC");
                query.setAttribute("maxRecords", "50");

                result = query.apply();

                if (!result.isError())
                {
                    int count = result.getItemCount();
                    for (int i = 0; i < count; i++)
                    {
                        Item item = result.getItemByIndex(i);
                        AddItemToList(item, "Document", workspace, innovator);
                    }
                }

                int totalItems = lvCheckedOut.Items.Count;
                if (totalItems == 0)
                {
                    SetStatus("No items locked by you.", Color.Orange);
                }
                else
                {
                    SetStatus($"Found {totalItems} locked item(s)", Color.Green);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message, Color.Red);
                MessageBox.Show("Failed to load locked items:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddItemToList(Item item, string itemType, string workspace, Innovator innovator)
        {
            string itemNumber = item.getProperty("item_number", "");
            string itemName = item.getProperty("name", "");
            string itemId = item.getID();
            string revision = item.getProperty("major_rev", "");
            string state = item.getProperty("state", "");

            // Try to find corresponding local file
            string localFile = "";
            string fileName = "";
            string fileId = "";

            // For Document items, get file from Document File relationship
            if (itemType == "Document")
            {
                try
                {
                    // Query Document File relationship to get the related File
                    Item docFileQuery = innovator.newItem("Document File", "get");
                    docFileQuery.setAttribute("select", "related_id");
                    docFileQuery.setProperty("source_id", itemId);
                    docFileQuery.setAttribute("maxRecords", "1");
                    docFileQuery.setAttribute("orderBy", "created_on DESC");
                    Item docFileResult = docFileQuery.apply();

                    if (!docFileResult.isError() && docFileResult.getItemCount() > 0)
                    {
                        fileId = docFileResult.getItemByIndex(0).getProperty("related_id", "");
                        if (!string.IsNullOrEmpty(fileId))
                        {
                            Item fileItem = innovator.getItemById("File", fileId);
                            if (fileItem != null && !fileItem.isError())
                            {
                                fileName = fileItem.getProperty("filename", "");
                            }
                        }
                    }
                }
                catch { }
            }
            else
            {
                // For CAD items, get file from native_file property
                fileId = item.getProperty("native_file", "");
                if (!string.IsNullOrEmpty(fileId))
                {
                    try
                    {
                        Item fileItem = innovator.getItemById("File", fileId);
                        if (fileItem != null && !fileItem.isError())
                        {
                            fileName = fileItem.getProperty("filename", "");
                        }
                    }
                    catch { }
                }
            }

            // If no filename, try common CATIA extensions
            if (string.IsNullOrEmpty(fileName))
            {
                string[] extensions = { ".CATPart", ".CATProduct", ".CATDrawing", ".cgr" };
                foreach (var ext in extensions)
                {
                    string testPath = Path.Combine(workspace, itemNumber + ext);
                    if (File.Exists(testPath))
                    {
                        fileName = itemNumber + ext;
                        break;
                    }
                }
            }

            // Check if local file exists
            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(workspace))
            {
                string testPath = Path.Combine(workspace, fileName);
                if (File.Exists(testPath))
                {
                    localFile = testPath;
                }
            }

            ListViewItem lvi = new ListViewItem(itemNumber);
            lvi.SubItems.Add(itemName);
            lvi.SubItems.Add(revision); // Revision
            lvi.SubItems.Add(state); // State
            lvi.SubItems.Add(itemType); // Type
            lvi.SubItems.Add(string.IsNullOrEmpty(localFile) ? "(not found locally)" : fileName);
            lvi.Tag = new CheckedOutItemInfo
            {
                ItemId = itemId,
                ItemNumber = itemNumber,
                ItemName = itemName,
                ItemType = itemType,
                LocalFilePath = localFile,
                FileName = fileName,
                OriginalFileId = fileId,
                Revision = revision,
                State = state
            };

            // Highlight based on state - can only check-in Preliminary items
            if (!string.IsNullOrEmpty(localFile))
            {
                if (state == "Preliminary")
                {
                    lvi.ForeColor = Color.Black;
                    lvi.Checked = true;
                }
                else
                {
                    lvi.ForeColor = Color.Orange; // Released/In Review - can't check in
                    lvi.Checked = false;
                }
            }
            else
            {
                lvi.ForeColor = Color.Gray;
            }

            lvCheckedOut.Items.Add(lvi);
        }

        private void LvCheckedOut_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // Enable check-in button if any item is checked
            bool anyChecked = false;
            foreach (ListViewItem item in lvCheckedOut.Items)
            {
                if (item.Checked)
                {
                    anyChecked = true;
                    break;
                }
            }
            btnCheckIn.Enabled = anyChecked;
        }

        private void BtnCheckIn_Click(object sender, EventArgs e)
        {
            if (!SessionManager.HasActiveSession)
            {
                MessageBox.Show("Session expired. Please login again.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get checked items
            List<ListViewItem> itemsToCheckIn = new List<ListViewItem>();
            foreach (ListViewItem item in lvCheckedOut.Items)
            {
                if (item.Checked)
                {
                    itemsToCheckIn.Add(item);
                }
            }

            if (itemsToCheckIn.Count == 0)
            {
                MessageBox.Show("Please select items to check in.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus("Checking in...", Color.Orange);
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            btnCheckIn.Enabled = false;
            Application.DoEvents();

            int successCount = 0;
            int failCount = 0;
            List<string> errors = new List<string>();
            List<string> successDetails = new List<string>();

            try
            {
                var innovator = SessionManager.Innovator;

                foreach (ListViewItem lvi in itemsToCheckIn)
                {
                    var info = lvi.Tag as CheckedOutItemInfo;
                    if (info == null) continue;

                    SetStatus($"Checking in {info.ItemNumber}...", Color.Orange);
                    Application.DoEvents();

                    try
                    {
                        // Validate state before allowing check-in
                        string currentState = info.State ?? "";
                        if (currentState != "Preliminary" && !string.IsNullOrEmpty(currentState))
                        {
                            string errorMsg;
                            switch (currentState)
                            {
                                case "Released":
                                    errorMsg = "Cannot check-in. Document is Released. Create new revision in Aras first.";
                                    break;
                                case "In Review":
                                    errorMsg = "Cannot check-in. Document is In Review.";
                                    break;
                                case "Obsolete":
                                    errorMsg = "Cannot check-in. Document is Obsolete.";
                                    break;
                                default:
                                    errorMsg = $"Cannot check-in. Document is in '{currentState}' state.";
                                    break;
                            }
                            throw new Exception(errorMsg);
                        }

                        // Upload new file if local file exists
                        if (!string.IsNullOrEmpty(info.LocalFilePath) && File.Exists(info.LocalFilePath))
                        {
                            SetStatus($"Uploading {info.FileName}...", Color.Orange);
                            Application.DoEvents();

                            string fileName = Path.GetFileName(info.LocalFilePath);
                            string absoluteFilePath = Path.GetFullPath(info.LocalFilePath);
                            bool fileUploaded = false;
                            string newFileId = null;

                            // Method 1: If original file exists, try to update it in place (proper versioning)
                            if (!string.IsNullOrEmpty(info.OriginalFileId))
                            {
                                try
                                {
                                    SetStatus($"Updating existing file (versioning)...", Color.Orange);
                                    Application.DoEvents();

                                    // Lock the File item first
                                    Item lockFile = innovator.newItem("File", "lock");
                                    lockFile.setID(info.OriginalFileId);
                                    Item lockFileResult = lockFile.apply();

                                    if (!lockFileResult.isError())
                                    {
                                        // Edit the existing file - attach new physical file
                                        Item editFile = innovator.newItem("File", "edit");
                                        editFile.setID(info.OriginalFileId);
                                        editFile.setProperty("filename", fileName);
                                        editFile.attachPhysicalFile(absoluteFilePath);
                                        Item editResult = editFile.apply();

                                        if (!editResult.isError())
                                        {
                                            newFileId = info.OriginalFileId;
                                            fileUploaded = true;
                                        }

                                        // Unlock the file if edit failed
                                        if (!fileUploaded)
                                        {
                                            Item unlockFile = innovator.newItem("File", "unlock");
                                            unlockFile.setID(info.OriginalFileId);
                                            unlockFile.apply();
                                        }
                                    }
                                }
                                catch { }
                            }

                            // Method 2: Create a new File item and add as relationship
                            if (!fileUploaded)
                            {
                                SetStatus($"Creating new file version...", Color.Orange);
                                Application.DoEvents();

                                Item fileItem = innovator.newItem("File", "add");
                                fileItem.setProperty("filename", fileName);
                                fileItem.attachPhysicalFile(absoluteFilePath);

                                Item fileResult = fileItem.apply();

                                if (fileResult.isError())
                                {
                                    throw new Exception("File upload error: " + fileResult.getErrorString());
                                }

                                newFileId = fileResult.getID();
                                fileUploaded = true;

                                SetStatus($"Linking file to item...", Color.Orange);
                                Application.DoEvents();

                                // Get the item to create relationship properly
                                Item sourceItem = innovator.getItemById(info.ItemType, info.ItemId);
                                if (sourceItem == null || sourceItem.isError())
                                {
                                    throw new Exception("Could not retrieve item to add file relationship");
                                }

                                bool fileLinked = false;

                                // Try CAD-specific relationship first (for CAD items)
                                if (info.ItemType == "CAD")
                                {
                                    try
                                    {
                                        // Try "CAD Document" relationship
                                        Item cadDocRel = sourceItem.createRelationship("CAD Document", "add");
                                        cadDocRel.setRelatedItem(fileResult);
                                        Item relResult = cadDocRel.apply();
                                        if (!relResult.isError())
                                        {
                                            fileLinked = true;
                                        }
                                    }
                                    catch { }

                                    // If CAD Document failed, try updating native_file property
                                    if (!fileLinked)
                                    {
                                        Item updateItem = innovator.newItem(info.ItemType, "edit");
                                        updateItem.setID(info.ItemId);
                                        updateItem.setProperty("native_file", newFileId);
                                        Item updateResult = updateItem.apply();
                                        if (!updateResult.isError())
                                        {
                                            fileLinked = true;
                                        }
                                    }
                                }
                                else if (info.ItemType == "Document")
                                {
                                    // Use Document File relationship for Document items
                                    try
                                    {
                                        Item docFileRel = sourceItem.createRelationship("Document File", "add");
                                        docFileRel.setRelatedItem(fileResult);
                                        Item relResult = docFileRel.apply();
                                        if (!relResult.isError())
                                        {
                                            fileLinked = true;
                                        }
                                    }
                                    catch { }
                                }

                                // Fallback: try generic relationship approach
                                if (!fileLinked)
                                {
                                    // Try setting source_id/related_id directly for Document File
                                    Item docFileRel = innovator.newItem("Document File", "add");
                                    docFileRel.setProperty("source_id", info.ItemId);
                                    docFileRel.setProperty("related_id", newFileId);
                                    Item relResult = docFileRel.apply();

                                    if (!relResult.isError())
                                    {
                                        fileLinked = true;
                                    }
                                }

                                // Last resort: update native_file property directly
                                if (!fileLinked)
                                {
                                    Item updateItem = innovator.newItem(info.ItemType, "edit");
                                    updateItem.setID(info.ItemId);
                                    updateItem.setProperty("native_file", newFileId);
                                    Item updateResult = updateItem.apply();

                                    if (updateResult.isError())
                                    {
                                        throw new Exception("Failed to link file to item: " + updateResult.getErrorString());
                                    }
                                }
                            }
                        }

                        // Unlock the item
                        SetStatus($"Unlocking {info.ItemNumber}...", Color.Orange);
                        Application.DoEvents();

                        Item unlockItem = innovator.newItem(info.ItemType, "unlock");
                        unlockItem.setID(info.ItemId);
                        Item unlockResult = unlockItem.apply();

                        if (unlockResult.isError())
                        {
                            throw new Exception("Failed to unlock: " + unlockResult.getErrorString());
                        }

                        // Close in CATIA if requested
                        if (chkCloseInCatia.Checked && !string.IsNullOrEmpty(info.LocalFilePath))
                        {
                            try
                            {
                                CloseDocumentInCatia(info.LocalFilePath);
                            }
                            catch { } // Ignore CATIA errors
                        }

                        successCount++;
                        successDetails.Add($"Document: {info.ItemNumber}\nRevision: {info.Revision}\nState: {info.State}\nStatus: Unlocked\nFile uploaded successfully");
                        lvi.ForeColor = Color.Green;
                        lvi.Checked = false;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errors.Add($"{info.ItemNumber} (Rev {info.Revision}): {ex.Message}");
                        lvi.ForeColor = Color.Red;
                    }
                }

                // Show results
                string message;
                if (successCount == 1 && failCount == 0)
                {
                    // Single item success - show detailed info
                    message = successDetails[0];
                }
                else
                {
                    // Multiple items or mixed results
                    message = $"Check-in complete.\n\nSuccess: {successCount}\nFailed: {failCount}";
                    if (errors.Count > 0)
                    {
                        message += "\n\nErrors:\n" + string.Join("\n", errors);
                    }
                }

                MessageBox.Show(message, "Check In Results",
                    MessageBoxButtons.OK,
                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (successCount > 0)
                {
                    SetStatus($"Checked in {successCount} item(s)", Color.Green);
                    // Refresh the list
                    LoadCheckedOutItems();
                }
                else
                {
                    SetStatus("Check-in failed", Color.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Check-in failed", Color.Red);
                MessageBox.Show("Check-in failed:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
                btnCheckIn.Enabled = true;
            }
        }

        private void CloseDocumentInCatia(string filePath)
        {
            try
            {
                Type catiaType = Type.GetTypeFromProgID("CATIA.Application");
                if (catiaType == null) return;

                object catia = System.Runtime.InteropServices.Marshal.GetActiveObject("CATIA.Application");
                if (catia == null) return;

                // Get Documents collection
                object documents = catiaType.InvokeMember("Documents",
                    System.Reflection.BindingFlags.GetProperty,
                    null, catia, null);

                if (documents == null) return;

                // Get document count
                int count = (int)documents.GetType().InvokeMember("Count",
                    System.Reflection.BindingFlags.GetProperty,
                    null, documents, null);

                string fileName = Path.GetFileName(filePath);

                // Find and close the document
                for (int i = count; i >= 1; i--)
                {
                    object doc = documents.GetType().InvokeMember("Item",
                        System.Reflection.BindingFlags.InvokeMethod,
                        null, documents, new object[] { i });

                    if (doc != null)
                    {
                        string docName = (string)doc.GetType().InvokeMember("Name",
                            System.Reflection.BindingFlags.GetProperty,
                            null, doc, null);

                        if (docName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Save and close
                            doc.GetType().InvokeMember("Save",
                                System.Reflection.BindingFlags.InvokeMethod,
                                null, doc, null);

                            doc.GetType().InvokeMember("Close",
                                System.Reflection.BindingFlags.InvokeMethod,
                                null, doc, null);

                            break;
                        }
                    }
                }
            }
            catch
            {
                // Ignore CATIA errors
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        // Helper class to store checked out item info
        private class CheckedOutItemInfo
        {
            public string ItemId { get; set; }
            public string ItemNumber { get; set; }
            public string ItemName { get; set; }
            public string ItemType { get; set; }
            public string LocalFilePath { get; set; }
            public string FileName { get; set; }
            public string OriginalFileId { get; set; }
            public string Revision { get; set; }
            public string State { get; set; }
        }
    }
}
