using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Aras.IOM;
using ArasCLI.Services;

namespace ArasCLI
{
    public class CatiaCheckOutForm : Form
    {
        // Controls
        private Label lblTitle;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private ListView lvResults;
        private Label lblWorkspace;
        private TextBox txtWorkspace;
        private Button btnBrowse;
        private Button btnCheckOut;
        private Button btnCancel;
        private Label lblStatus;
        private Panel statusPanel;
        private ProgressBar progressBar;

        // Services
        private ConfigManager _configManager;
        private string _foundItemType = "CAD"; // Track which item type was found in search

        public CatiaCheckOutForm()
        {
            _configManager = new ConfigManager();
            InitializeComponents();
            CheckSession();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Aras Innovator - Check Out";
            this.Size = new Size(700, 550);
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
                Text = "Check Out from Aras",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(leftMargin, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);
            yPos += 45;

            // Search
            lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(60, 20)
            };
            txtSearch = new TextBox
            {
                Location = new Point(leftMargin + 65, yPos),
                Size = new Size(formWidth - 150, 25)
            };
            txtSearch.KeyPress += TxtSearch_KeyPress;

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(leftMargin + formWidth - 80, yPos),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnSearch);
            yPos += 35;

            // Results ListView
            lvResults = new ListView
            {
                Location = new Point(leftMargin, yPos),
                Size = new Size(formWidth, 250),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            lvResults.Columns.Add("Item Number", 100);
            lvResults.Columns.Add("Name", 150);
            lvResults.Columns.Add("Rev", 40);
            lvResults.Columns.Add("State", 80);
            lvResults.Columns.Add("Type", 70);
            lvResults.Columns.Add("Locked By", 100);
            lvResults.DoubleClick += LvResults_DoubleClick;

            this.Controls.Add(lvResults);
            yPos += 260;

            // Workspace
            lblWorkspace = new Label
            {
                Text = "Download to:",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(80, 20)
            };
            txtWorkspace = new TextBox
            {
                Location = new Point(leftMargin + 85, yPos),
                Size = new Size(formWidth - 170, 25),
                Text = _configManager.Config.LocalWorkspace
            };
            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new Point(leftMargin + formWidth - 80, yPos),
                Size = new Size(80, 25)
            };
            btnBrowse.Click += BtnBrowse_Click;

            this.Controls.Add(lblWorkspace);
            this.Controls.Add(txtWorkspace);
            this.Controls.Add(btnBrowse);
            yPos += 40;

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
            btnCheckOut = new Button
            {
                Text = "Check Out",
                Location = new Point(leftMargin + formWidth - 220, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnCheckOut.FlatAppearance.BorderSize = 0;
            btnCheckOut.Click += BtnCheckOut_Click;

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

            this.Controls.Add(btnCheckOut);
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

            // Accept button
            this.AcceptButton = btnSearch;
            this.CancelButton = btnCancel;
        }

        private void CheckSession()
        {
            if (!SessionManager.HasActiveSession)
            {
                SetStatus("Not logged in. Please login first.", Color.Red);
                btnSearch.Enabled = false;
                txtSearch.Enabled = false;

                // Prompt to login
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
                            btnSearch.Enabled = true;
                            txtSearch.Enabled = true;
                            txtSearch.Focus();
                        }
                    }
                }
            }
            else
            {
                SetStatus($"Connected as: {SessionManager.Username}", Color.Green);
                txtSearch.Focus();
            }
        }

        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BtnSearch_Click(sender, e);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (!SessionManager.HasActiveSession)
            {
                MessageBox.Show("Please login first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = "*"; // Search all
            }

            SetStatus("Searching...", Color.Orange);
            lvResults.Items.Clear();
            btnCheckOut.Enabled = false;
            Application.DoEvents();

            try
            {
                var innovator = SessionManager.Innovator;
                int totalCount = 0;

                // Search for Document items first
                Item docQuery = innovator.newItem("Document", "get");
                docQuery.setAttribute("select", "item_number,name,major_rev,state,locked_by_id");

                if (searchTerm != "*")
                {
                    docQuery.setProperty("item_number", $"%{searchTerm}%");
                    docQuery.setPropertyCondition("item_number", "like");
                }

                docQuery.setAttribute("orderBy", "modified_on DESC");
                docQuery.setAttribute("maxRecords", "50");

                Item docResult = docQuery.apply();

                if (!docResult.isError())
                {
                    int count = docResult.getItemCount();
                    for (int i = 0; i < count; i++)
                    {
                        Item item = docResult.getItemByIndex(i);
                        AddItemToResults(item, "Document", innovator);
                        totalCount++;
                    }
                }

                // Also search for CAD items
                Item cadQuery = innovator.newItem("CAD", "get");
                cadQuery.setAttribute("select", "item_number,name,major_rev,state,locked_by_id");

                if (searchTerm != "*")
                {
                    cadQuery.setProperty("item_number", $"%{searchTerm}%");
                    cadQuery.setPropertyCondition("item_number", "like");
                }

                cadQuery.setAttribute("orderBy", "modified_on DESC");
                cadQuery.setAttribute("maxRecords", "50");

                Item cadResult = cadQuery.apply();

                if (!cadResult.isError())
                {
                    int count = cadResult.getItemCount();
                    for (int i = 0; i < count; i++)
                    {
                        Item item = cadResult.getItemByIndex(i);
                        AddItemToResults(item, "CAD", innovator);
                        totalCount++;
                    }
                }

                if (totalCount == 0)
                {
                    SetStatus("Search completed - no items found", Color.Orange);
                    return;
                }

                SetStatus($"Found {totalCount} item(s)", Color.Green);

                if (totalCount > 0)
                {
                    lvResults.Items[0].Selected = true;
                    btnCheckOut.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                SetStatus("Search failed: " + ex.Message, Color.Red);
                MessageBox.Show("Search failed:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddItemToResults(Item item, string itemType, Innovator innovator)
        {
            ListViewItem lvi = new ListViewItem(item.getProperty("item_number", ""));
            lvi.SubItems.Add(item.getProperty("name", ""));
            lvi.SubItems.Add(item.getProperty("major_rev", "")); // Revision
            lvi.SubItems.Add(item.getProperty("state", "")); // State
            lvi.SubItems.Add(itemType); // Type

            // Get locked by username
            string lockedById = item.getProperty("locked_by_id", "");
            string lockedByName = "";
            if (!string.IsNullOrEmpty(lockedById))
            {
                try
                {
                    Item lockedByUser = innovator.getItemById("User", lockedById);
                    if (lockedByUser != null && !lockedByUser.isError())
                    {
                        lockedByName = lockedByUser.getProperty("keyed_name", lockedById);
                    }
                    else
                    {
                        lockedByName = lockedById;
                    }
                }
                catch
                {
                    lockedByName = lockedById;
                }
            }
            lvi.SubItems.Add(lockedByName); // Locked By

            // Store item info for checkout
            lvi.Tag = new SearchResultInfo
            {
                ItemId = item.getID(),
                ItemType = itemType,
                LockedById = lockedById,
                LockedByName = lockedByName,
                Revision = item.getProperty("major_rev", ""),
                State = item.getProperty("state", "")
            };

            // Highlight locked items
            if (!string.IsNullOrEmpty(lockedById))
            {
                string currentUserId = innovator.getUserID();
                if (lockedById == currentUserId)
                {
                    lvi.ForeColor = Color.Blue; // Locked by me
                }
                else
                {
                    lvi.ForeColor = Color.Red; // Locked by someone else
                }
            }

            lvResults.Items.Add(lvi);
        }

        private void LvResults_DoubleClick(object sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count > 0)
            {
                BtnCheckOut_Click(sender, e);
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select download folder";
                dialog.SelectedPath = txtWorkspace.Text;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtWorkspace.Text = dialog.SelectedPath;

                    // Save to config
                    _configManager.Config.LocalWorkspace = dialog.SelectedPath;
                    _configManager.Save();
                }
            }
        }

        private void BtnCheckOut_Click(object sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an item to check out.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SessionManager.HasActiveSession)
            {
                MessageBox.Show("Session expired. Please login again.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string workspace = txtWorkspace.Text.Trim();
            if (string.IsNullOrEmpty(workspace))
            {
                MessageBox.Show("Please specify a download folder.", "No Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(workspace))
            {
                var result = MessageBox.Show(
                    $"Folder does not exist:\n{workspace}\n\nCreate it?",
                    "Create Folder",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(workspace);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to create folder:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            ListViewItem selected = lvResults.SelectedItems[0];
            var resultInfo = selected.Tag as SearchResultInfo;
            string itemId = resultInfo?.ItemId;
            string itemType = resultInfo?.ItemType ?? _foundItemType;
            string itemNumber = selected.Text;
            string itemName = selected.SubItems[1].Text;
            string revision = resultInfo?.Revision ?? "";
            string state = resultInfo?.State ?? "";

            // Update _foundItemType for compatibility with rest of checkout code
            _foundItemType = itemType;

            SetStatus($"Checking out {itemNumber}...", Color.Orange);
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            btnCheckOut.Enabled = false;
            Application.DoEvents();

            try
            {
                // Verify session is still active
                if (!SessionManager.HasActiveSession)
                {
                    throw new Exception("Session expired. Please login again.");
                }

                var innovator = SessionManager.Innovator;
                if (innovator == null)
                {
                    throw new Exception("Not connected to Aras. Please login first.");
                }

                // Check if item is already locked by someone else (using cached info first)
                string currentUserId = innovator.getUserID();
                string lockedById = resultInfo?.LockedById ?? "";
                string lockedByName = resultInfo?.LockedByName ?? "";

                if (!string.IsNullOrEmpty(lockedById) && lockedById != currentUserId)
                {
                    throw new Exception($"Document is locked by: {lockedByName}\n\nCannot check out an item that is locked by another user.");
                }

                bool alreadyLockedByMe = (!string.IsNullOrEmpty(lockedById) && lockedById == currentUserId);

                // Lock the item in Aras if not already locked by me
                if (!alreadyLockedByMe)
                {
                    SetStatus($"Locking {itemNumber} in Aras...", Color.Orange);
                    Application.DoEvents();

                    Item lockItem = innovator.newItem(_foundItemType, "lock");
                    lockItem.setID(itemId);
                    Item lockResult = lockItem.apply();

                    if (lockResult.isError())
                    {
                        string lockError = lockResult.getErrorString();
                        throw new Exception($"Failed to lock item: {lockError}");
                    }
                }

                // Get the item using the type that was found during search
                Item item = innovator.getItemById(_foundItemType, itemId);
                if (item == null)
                {
                    throw new Exception($"Failed to get item: null response for {_foundItemType} ID {itemId}");
                }
                if (item.isError())
                {
                    throw new Exception($"Failed to get item ({_foundItemType}): " + item.getErrorString());
                }

                // Get file info - approach depends on item type
                string fileName = null;
                string fileId = null;

                if (itemType == "Document")
                {
                    // For Document items, get file from Document File relationship (latest version)
                    try
                    {
                        Item docFileQuery = innovator.newItem("Document File", "get");
                        docFileQuery.setAttribute("select", "related_id");
                        docFileQuery.setProperty("source_id", itemId);
                        docFileQuery.setAttribute("maxRecords", "1");
                        docFileQuery.setAttribute("orderBy", "created_on DESC"); // Get latest file
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
                    // For CAD items, try native_file property first
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

                    // If no file yet, try CAD Document relationship
                    if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(fileName))
                    {
                        try
                        {
                            Item cadDocQuery = innovator.newItem("CAD Document", "get");
                            cadDocQuery.setAttribute("select", "related_id");
                            cadDocQuery.setProperty("source_id", itemId);
                            cadDocQuery.setAttribute("maxRecords", "1");
                            cadDocQuery.setAttribute("orderBy", "created_on DESC");
                            Item cadDocResult = cadDocQuery.apply();

                            if (!cadDocResult.isError() && cadDocResult.getItemCount() > 0)
                            {
                                fileId = cadDocResult.getItemByIndex(0).getProperty("related_id", "");
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
                }

                // Default filename if none found
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = itemNumber + ".CATPart";
                }

                string localFilePath = Path.Combine(workspace, fileName);
                bool fileDownloaded = false;

                // Try to download the file if we have a file ID
                if (!string.IsNullOrEmpty(fileId))
                {
                    try
                    {
                        SetStatus($"Downloading {fileName}...", Color.Orange);
                        Application.DoEvents();

                        // Get file URL from vault
                        var connection = SessionManager.Connection;
                        if (connection != null)
                        {
                            string fileUrl = connection.getFileUrl(fileId, UrlType.SecurityToken);

                            if (!string.IsNullOrEmpty(fileUrl))
                            {
                                // Download file
                                using (var client = new System.Net.WebClient())
                                {
                                    client.DownloadFile(fileUrl, localFilePath);
                                }
                                fileDownloaded = true;
                            }
                        }
                    }
                    catch (Exception downloadEx)
                    {
                        // File download failed, but continue with checkout registration
                        SetStatus($"Download failed: {downloadEx.Message}", Color.Orange);
                    }
                }

                // Show result and open in CATIA
                if (fileDownloaded)
                {
                    SetStatus($"Opening {fileName} in CATIA...", Color.Orange);
                    Application.DoEvents();

                    // Try to open in running CATIA instance
                    bool openedInCatia = false;
                    string openError = null;

                    try
                    {
                        // Get running CATIA instance via COM
                        Type catiaType = Type.GetTypeFromProgID("CATIA.Application");
                        if (catiaType != null)
                        {
                            object catia = System.Runtime.InteropServices.Marshal.GetActiveObject("CATIA.Application");
                            if (catia != null)
                            {
                                // Get Documents collection and open file
                                object documents = catiaType.InvokeMember("Documents",
                                    System.Reflection.BindingFlags.GetProperty,
                                    null, catia, null);

                                if (documents != null)
                                {
                                    documents.GetType().InvokeMember("Open",
                                        System.Reflection.BindingFlags.InvokeMethod,
                                        null, documents, new object[] { localFilePath });

                                    openedInCatia = true;

                                    // Don't release COM objects - let CATIA manage them
                                }
                            }
                        }
                    }
                    catch (System.Runtime.InteropServices.COMException comEx)
                    {
                        openError = "CATIA COM error: " + comEx.Message;
                    }
                    catch (Exception ex)
                    {
                        openError = ex.Message;
                    }

                    if (openedInCatia)
                    {
                        MessageBox.Show(
                            $"Document: {itemNumber}\nRevision: {revision}\nState: {state}\nStatus: Locked by you\n\nFile: {fileName}\nFile downloaded and opened in CATIA.",
                            "Check Out Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        SetStatus($"Checked out: {itemNumber} Rev {revision} ({state}) - Opened in CATIA", Color.Green);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Document: {itemNumber}\nRevision: {revision}\nState: {state}\nStatus: Locked by you\n\nFile: {fileName}\nFile downloaded to:\n{localFilePath}\n\nCould not open in CATIA: {openError}",
                            "Check Out Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        SetStatus($"Checked out: {itemNumber} Rev {revision} ({state}) - File downloaded", Color.Green);
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"Document: {itemNumber}\nRevision: {revision}\nState: {state}\nStatus: Locked by you\n\nNo file attached or download not available.",
                        "Check Out",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    SetStatus($"Checked out: {itemNumber} Rev {revision} ({state}) (no file)", Color.Orange);
                }

                // Save workspace to config
                _configManager.Config.LocalWorkspace = workspace;
                _configManager.Save();
            }
            catch (Exception ex)
            {
                SetStatus("Check-out failed", Color.Red);
                MessageBox.Show("Check-out failed:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
                btnCheckOut.Enabled = true;
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

        // Helper class to store search result info
        private class SearchResultInfo
        {
            public string ItemId { get; set; }
            public string ItemType { get; set; }
            public string LockedById { get; set; }
            public string LockedByName { get; set; }
            public string Revision { get; set; }
            public string State { get; set; }
        }
    }
}
