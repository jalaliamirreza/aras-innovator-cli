using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using ArasCLI.Services;

namespace ArasCLI
{
    /// <summary>
    /// Main form for CATIA-Aras integration.
    /// Provides Check-in, Check-out, and Get Latest functionality.
    /// </summary>
    public class CatiaIntegrationForm : Form
    {
        // Services
        private ArasService _arasService;
        private CatiaService _catiaService;
        private ConfigManager _configManager;

        // Connection controls
        private TextBox txtUrl;
        private TextBox txtDatabase;
        private TextBox txtUser;
        private TextBox txtPassword;
        private Button btnConnect;
        private Label lblConnectionStatus;

        // CATIA status controls
        private Label lblCatiaStatus;
        private Button btnRefreshCatia;
        private TextBox txtActiveDocument;
        private TextBox txtPartNumber;

        // Action buttons
        private Button btnCheckin;
        private Button btnCheckout;
        private Button btnGetLatest;
        private Button btnSearch;

        // Document info controls
        private TextBox txtDocumentName;
        private TextBox txtItemNumber;
        private ComboBox cboDocType;

        // Search/Results controls
        private DataGridView dgvResults;
        private TextBox txtSearch;
        private Button btnDoSearch;

        // Log
        private RichTextBox txtLog;

        // Status
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;

        public CatiaIntegrationForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Aras Innovator - CATIA Integration";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);

            // Main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Connection panel
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Main content
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Log panel

            // Connection Panel (spans both columns)
            GroupBox grpConnection = CreateConnectionPanel();
            mainLayout.Controls.Add(grpConnection, 0, 0);
            mainLayout.SetColumnSpan(grpConnection, 2);

            // Left Panel - CATIA Status & Actions
            GroupBox grpCatia = CreateCatiaPanel();
            mainLayout.Controls.Add(grpCatia, 0, 1);

            // Right Panel - Search & Results
            GroupBox grpSearch = CreateSearchPanel();
            mainLayout.Controls.Add(grpSearch, 1, 1);

            // Log Panel (spans both columns)
            GroupBox grpLog = CreateLogPanel();
            mainLayout.Controls.Add(grpLog, 0, 2);
            mainLayout.SetColumnSpan(grpLog, 2);

            this.Controls.Add(mainLayout);

            // Status bar
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Ready");
            statusStrip.Items.Add(lblStatus);
            this.Controls.Add(statusStrip);
        }

        private GroupBox CreateConnectionPanel()
        {
            GroupBox grp = new GroupBox
            {
                Text = "Aras Connection",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Row 1: URL and Database
            layout.Controls.Add(new Label { Text = "Server URL:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            txtUrl = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtUrl, 1, 0);

            layout.Controls.Add(new Label { Text = "Database:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 0);
            txtDatabase = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtDatabase, 3, 0);

            // Row 2: User and Password
            layout.Controls.Add(new Label { Text = "Username:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            txtUser = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtUser, 1, 1);

            layout.Controls.Add(new Label { Text = "Password:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 1);
            txtPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            layout.Controls.Add(txtPassword, 3, 1);

            // Row 3: Connect button and status
            btnConnect = new Button { Text = "Connect", Width = 100, Height = 30 };
            btnConnect.Click += BtnConnect_Click;
            layout.Controls.Add(btnConnect, 0, 2);

            lblConnectionStatus = new Label
            {
                Text = "Not connected",
                ForeColor = Color.Red,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(lblConnectionStatus, 1, 2);

            // CATIA status
            lblCatiaStatus = new Label
            {
                Text = "CATIA: Not connected",
                ForeColor = Color.Gray,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(lblCatiaStatus, 2, 2);

            btnRefreshCatia = new Button { Text = "Refresh", Width = 80 };
            btnRefreshCatia.Click += BtnRefreshCatia_Click;
            layout.Controls.Add(btnRefreshCatia, 3, 2);

            grp.Controls.Add(layout);
            return grp;
        }

        private GroupBox CreateCatiaPanel()
        {
            GroupBox grp = new GroupBox
            {
                Text = "CATIA Document & Actions",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Active Document
            layout.Controls.Add(new Label { Text = "Active Doc:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            txtActiveDocument = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = SystemColors.Info };
            layout.Controls.Add(txtActiveDocument, 1, 0);

            // Part Number
            layout.Controls.Add(new Label { Text = "Part Number:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            txtPartNumber = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = SystemColors.Info };
            layout.Controls.Add(txtPartNumber, 1, 1);

            // Document Name (for check-in)
            layout.Controls.Add(new Label { Text = "Doc Name:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
            txtDocumentName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtDocumentName, 1, 2);

            // Item Number (for check-in)
            layout.Controls.Add(new Label { Text = "Item Number:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 3);
            txtItemNumber = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtItemNumber, 1, 3);

            // Doc Type
            layout.Controls.Add(new Label { Text = "Doc Type:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 4);
            cboDocType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboDocType.Items.AddRange(new[] { "3D Model", "Drawing", "Exchange", "Other" });
            cboDocType.SelectedIndex = 0;
            layout.Controls.Add(cboDocType, 1, 4);

            // Action Buttons
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            btnCheckin = new Button { Text = "Check-In", Width = 100, Height = 35, BackColor = Color.LightGreen };
            btnCheckin.Click += BtnCheckin_Click;
            buttonPanel.Controls.Add(btnCheckin);

            btnCheckout = new Button { Text = "Check-Out", Width = 100, Height = 35, BackColor = Color.LightBlue };
            btnCheckout.Click += BtnCheckout_Click;
            buttonPanel.Controls.Add(btnCheckout);

            btnGetLatest = new Button { Text = "Get Latest", Width = 100, Height = 35, BackColor = Color.LightYellow };
            btnGetLatest.Click += BtnGetLatest_Click;
            buttonPanel.Controls.Add(btnGetLatest);

            Button btnUnlock = new Button { Text = "Unlock", Width = 80, Height = 35, BackColor = Color.LightCoral };
            btnUnlock.Click += BtnUnlock_Click;
            buttonPanel.Controls.Add(btnUnlock);

            layout.Controls.Add(buttonPanel, 0, 5);
            layout.SetColumnSpan(buttonPanel, 2);

            // Help text
            Label lblHelp = new Label
            {
                Text = "Check-In: Upload CATIA file to Aras (file browser)\n" +
                       "Check-Out: Download & lock document for editing\n" +
                       "Get Latest: Download latest version (read-only)\n" +
                       "Unlock: Release lock on selected document",
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray,
                AutoSize = false
            };
            layout.Controls.Add(lblHelp, 0, 6);
            layout.SetColumnSpan(lblHelp, 2);

            grp.Controls.Add(layout);
            return grp;
        }

        private GroupBox CreateSearchPanel()
        {
            GroupBox grp = new GroupBox
            {
                Text = "Search Aras Documents",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Search box
            txtSearch = new TextBox { Dock = DockStyle.Fill };
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnDoSearch_Click(s, e); };
            layout.Controls.Add(txtSearch, 0, 0);

            btnDoSearch = new Button { Text = "Search", Dock = DockStyle.Fill };
            btnDoSearch.Click += BtnDoSearch_Click;
            layout.Controls.Add(btnDoSearch, 1, 0);

            // Results grid
            dgvResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvResults.Columns.Add("ItemNumber", "Item Number");
            dgvResults.Columns.Add("Name", "Name");
            dgvResults.Columns.Add("State", "State");
            dgvResults.Columns.Add("LockedBy", "Locked By");
            dgvResults.Columns.Add("ID", "ID");
            dgvResults.Columns["ID"].Visible = false;
            dgvResults.CellDoubleClick += DgvResults_CellDoubleClick;

            layout.Controls.Add(dgvResults, 0, 1);
            layout.SetColumnSpan(dgvResults, 2);

            grp.Controls.Add(layout);
            return grp;
        }

        private GroupBox CreateLogPanel()
        {
            GroupBox grp = new GroupBox
            {
                Text = "Log",
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            txtLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen
            };

            grp.Controls.Add(txtLog);
            return grp;
        }

        private void InitializeServices()
        {
            _configManager = new ConfigManager();
            _arasService = new ArasService();
            _catiaService = new CatiaService();
        }

        private void LoadSettings()
        {
            txtUrl.Text = _configManager.Config.ArasServerUrl;
            txtDatabase.Text = _configManager.Config.ArasDatabase;
            txtUser.Text = _configManager.Config.ArasUsername;

            RefreshCatiaStatus();
        }

        private void SaveSettings()
        {
            _configManager.Config.ArasServerUrl = txtUrl.Text;
            _configManager.Config.ArasDatabase = txtDatabase.Text;
            _configManager.Config.ArasUsername = txtUser.Text;
            _configManager.Save();
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            SetStatus("Connecting to Aras...");

            await Task.Run(() =>
            {
                bool success = _arasService.Connect(
                    txtUrl.Text,
                    txtDatabase.Text,
                    txtUser.Text,
                    txtPassword.Text,
                    out string errorMessage);

                this.Invoke((Action)(() =>
                {
                    if (success)
                    {
                        lblConnectionStatus.Text = "Connected";
                        lblConnectionStatus.ForeColor = Color.Green;
                        Log("Connected to Aras: " + txtUrl.Text);
                        SaveSettings();
                    }
                    else
                    {
                        lblConnectionStatus.Text = "Connection failed";
                        lblConnectionStatus.ForeColor = Color.Red;
                        Log("Connection failed: " + errorMessage, LogLevel.Error);
                    }
                    btnConnect.Enabled = true;
                    SetStatus("Ready");
                }));
            });
        }

        private void BtnRefreshCatia_Click(object sender, EventArgs e)
        {
            RefreshCatiaStatus();
        }

        private void RefreshCatiaStatus()
        {
            if (_catiaService.Connect(out string errorMessage))
            {
                lblCatiaStatus.Text = "CATIA: Connected";
                lblCatiaStatus.ForeColor = Color.Green;

                var docInfo = _catiaService.GetActiveDocument(out string docError);
                if (docInfo != null)
                {
                    txtActiveDocument.Text = docInfo.Name;
                    txtPartNumber.Text = docInfo.PartNumber ?? "";
                    txtDocumentName.Text = docInfo.Nomenclature ?? docInfo.Name;
                    txtItemNumber.Text = docInfo.PartNumber ?? "";
                    Log($"Active CATIA document: {docInfo.Name} ({docInfo.DocumentType})");
                }
                else
                {
                    txtActiveDocument.Text = "(No document open)";
                    txtPartNumber.Text = "";
                }
            }
            else
            {
                lblCatiaStatus.Text = "CATIA: Not running";
                lblCatiaStatus.ForeColor = Color.Gray;
                txtActiveDocument.Text = "";
                txtPartNumber.Text = "";
            }
        }

        private async void BtnCheckin_Click(object sender, EventArgs e)
        {
            if (!_arasService.IsConnected)
            {
                MessageBox.Show("Please connect to Aras first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filePath = null;
            string itemNumber = txtItemNumber.Text;
            string documentName = txtDocumentName.Text;

            // First try to get active document from CATIA (if COM works)
            var docInfo = _catiaService.GetActiveDocument(out string docError);
            if (docInfo != null)
            {
                // CATIA COM works - try to save and use active document
                SetStatus("Saving CATIA document...");
                if (_catiaService.SaveActiveDocument(out string saveError))
                {
                    filePath = docInfo.FullPath;
                    Log($"Using active CATIA document: {filePath}");
                }
                else
                {
                    Log($"Could not save via COM, using file browser: {saveError}", LogLevel.Warning);
                }
            }
            else
            {
                Log($"CATIA COM not available: {docError}", LogLevel.Warning);
            }

            // If CATIA COM didn't work, use file browser
            if (string.IsNullOrEmpty(filePath))
            {
                Log("Opening file browser for Check-In...");

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Select CATIA File to Check-In";
                    ofd.Filter = "CATIA Files|*.CATPart;*.CATProduct;*.CATDrawing|" +
                                 "CATIA Part (*.CATPart)|*.CATPart|" +
                                 "CATIA Product (*.CATProduct)|*.CATProduct|" +
                                 "CATIA Drawing (*.CATDrawing)|*.CATDrawing|" +
                                 "All Files (*.*)|*.*";
                    ofd.InitialDirectory = _configManager.Config.LocalWorkspace;

                    if (ofd.ShowDialog() != DialogResult.OK)
                    {
                        Log("Check-In cancelled by user.");
                        return;
                    }

                    filePath = ofd.FileName;

                    // Auto-fill item number and name from filename if empty
                    if (string.IsNullOrWhiteSpace(itemNumber))
                    {
                        itemNumber = Path.GetFileNameWithoutExtension(filePath);
                        txtItemNumber.Text = itemNumber;
                    }
                    if (string.IsNullOrWhiteSpace(documentName))
                    {
                        documentName = Path.GetFileNameWithoutExtension(filePath);
                        txtDocumentName.Text = documentName;
                    }
                }
            }

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("No valid file selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnCheckin.Enabled = false;
            SetStatus("Checking in to Aras...");
            Log($"Check-in started: {filePath}");

            await Task.Run(() =>
            {
                var result = _arasService.CreateDocumentWithFile(filePath, itemNumber, documentName, out string error);

                this.Invoke((Action)(() =>
                {
                    if (result != null && !result.isError())
                    {
                        Log($"Check-in successful! Document ID: {result.getID()}", LogLevel.Success);
                        MessageBox.Show("Document checked in successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Log($"Check-in failed: {error}", LogLevel.Error);
                        MessageBox.Show("Check-in failed: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    btnCheckin.Enabled = true;
                    SetStatus("Ready");
                }));
            });
        }

        private async void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (!_arasService.IsConnected)
            {
                MessageBox.Show("Please connect to Aras first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvResults.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a document from the search results.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string documentId = dgvResults.SelectedRows[0].Cells["ID"].Value?.ToString();
            string itemNumber = dgvResults.SelectedRows[0].Cells["ItemNumber"].Value?.ToString();

            if (string.IsNullOrEmpty(documentId))
            {
                MessageBox.Show("Invalid document selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnCheckout.Enabled = false;
            SetStatus("Checking out from Aras...");
            Log($"Check-out started: {itemNumber}");

            await Task.Run(() =>
            {
                // Lock the document
                var lockResult = _arasService.LockItem("Document", documentId, out string lockError);
                if (lockResult == null || lockResult.isError())
                {
                    this.Invoke((Action)(() =>
                    {
                        Log($"Could not lock document: {lockError}", LogLevel.Error);
                        MessageBox.Show("Could not lock document: " + lockError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnCheckout.Enabled = true;
                        SetStatus("Ready");
                    }));
                    return;
                }

                // Get document with files
                var docResult = _arasService.GetDocumentWithFiles(documentId, out string docError);
                if (docResult == null || docResult.isError())
                {
                    this.Invoke((Action)(() =>
                    {
                        Log($"Could not get document: {docError}", LogLevel.Error);
                        btnCheckout.Enabled = true;
                        SetStatus("Ready");
                    }));
                    return;
                }

                // Download file
                var docFiles = docResult.getRelationships("Document File");
                if (docFiles != null && docFiles.getItemCount() > 0)
                {
                    var relatedFile = docFiles.getItemByIndex(0).getRelatedItem();
                    if (relatedFile != null)
                    {
                        string fileId = relatedFile.getID();
                        string fileName = relatedFile.getProperty("filename");
                        string localPath = Path.Combine(_configManager.Config.LocalWorkspace, fileName);

                        // Ensure workspace exists
                        Directory.CreateDirectory(_configManager.Config.LocalWorkspace);

                        bool downloaded = _arasService.DownloadFile(fileId, localPath, out string downloadError);

                        this.Invoke((Action)(() =>
                        {
                            if (downloaded)
                            {
                                Log($"File downloaded to: {localPath}", LogLevel.Success);

                                // Open in CATIA
                                if (_catiaService.OpenDocument(localPath, out string openError))
                                {
                                    if (string.IsNullOrEmpty(openError))
                                    {
                                        Log($"Opened in CATIA: {fileName}");
                                    }
                                    else
                                    {
                                        Log($"Opening file: {openError}", LogLevel.Warning);
                                    }
                                    RefreshCatiaStatus();
                                }
                                else
                                {
                                    Log($"Could not open in CATIA: {openError}", LogLevel.Warning);
                                }

                                MessageBox.Show($"Document checked out!\n\nPath: {localPath}\n\nPlease verify the file is open in CATIA.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                Log($"Download failed: {downloadError}", LogLevel.Error);
                                MessageBox.Show("Download failed: " + downloadError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            btnCheckout.Enabled = true;
                            SetStatus("Ready");
                        }));
                    }
                }
                else
                {
                    this.Invoke((Action)(() =>
                    {
                        Log("No files attached to document.", LogLevel.Warning);
                        MessageBox.Show("No files attached to this document.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        btnCheckout.Enabled = true;
                        SetStatus("Ready");
                    }));
                }
            });
        }

        private async void BtnGetLatest_Click(object sender, EventArgs e)
        {
            if (!_arasService.IsConnected)
            {
                MessageBox.Show("Please connect to Aras first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvResults.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a document from the search results.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string documentId = dgvResults.SelectedRows[0].Cells["ID"].Value?.ToString();
            string itemNumber = dgvResults.SelectedRows[0].Cells["ItemNumber"].Value?.ToString();

            btnGetLatest.Enabled = false;
            SetStatus("Getting latest from Aras...");
            Log($"Get Latest started: {itemNumber}");

            await Task.Run(() =>
            {
                // Get document with files (no lock)
                var docResult = _arasService.GetDocumentWithFiles(documentId, out string docError);
                if (docResult == null || docResult.isError())
                {
                    this.Invoke((Action)(() =>
                    {
                        Log($"Could not get document: {docError}", LogLevel.Error);
                        btnGetLatest.Enabled = true;
                        SetStatus("Ready");
                    }));
                    return;
                }

                // Download file
                var docFiles = docResult.getRelationships("Document File");
                if (docFiles != null && docFiles.getItemCount() > 0)
                {
                    var relatedFile = docFiles.getItemByIndex(0).getRelatedItem();
                    if (relatedFile != null)
                    {
                        string fileId = relatedFile.getID();
                        string fileName = relatedFile.getProperty("filename");
                        string localPath = Path.Combine(_configManager.Config.LocalWorkspace, fileName);

                        Directory.CreateDirectory(_configManager.Config.LocalWorkspace);

                        bool downloaded = _arasService.DownloadFile(fileId, localPath, out string downloadError);

                        this.Invoke((Action)(() =>
                        {
                            if (downloaded)
                            {
                                Log($"File downloaded to: {localPath}", LogLevel.Success);

                                // Make file read-only since it's not checked out
                                try
                                {
                                    File.SetAttributes(localPath, File.GetAttributes(localPath) | FileAttributes.ReadOnly);
                                }
                                catch { }

                                // Open in CATIA
                                if (_catiaService.OpenDocument(localPath, out string openError))
                                {
                                    Log($"Opened in CATIA (read-only): {fileName}");
                                    RefreshCatiaStatus();
                                }

                                MessageBox.Show($"Latest version downloaded!\n\nPath: {localPath}\n\nNote: File is read-only (not checked out)", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                Log($"Download failed: {downloadError}", LogLevel.Error);
                                MessageBox.Show("Download failed: " + downloadError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            btnGetLatest.Enabled = true;
                            SetStatus("Ready");
                        }));
                    }
                }
                else
                {
                    this.Invoke((Action)(() =>
                    {
                        Log("No files attached to document.", LogLevel.Warning);
                        btnGetLatest.Enabled = true;
                        SetStatus("Ready");
                    }));
                }
            });
        }

        private async void BtnDoSearch_Click(object sender, EventArgs e)
        {
            if (!_arasService.IsConnected)
            {
                MessageBox.Show("Please connect to Aras first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvResults.Rows.Clear();
            SetStatus("Searching...");

            await Task.Run(() =>
            {
                var result = _arasService.SearchDocuments(txtSearch.Text, out string error);

                this.Invoke((Action)(() =>
                {
                    if (result != null && !result.isError())
                    {
                        int count = result.getItemCount();
                        for (int i = 0; i < count; i++)
                        {
                            var item = result.getItemByIndex(i);
                            dgvResults.Rows.Add(
                                item.getProperty("item_number"),
                                item.getProperty("name"),
                                item.getProperty("state"),
                                item.getProperty("locked_by_id"),
                                item.getID()
                            );
                        }
                        Log($"Search returned {count} result(s)");
                    }
                    else
                    {
                        Log($"Search error: {error}", LogLevel.Error);
                    }
                    SetStatus("Ready");
                }));
            });
        }

        private void DgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Double-click to check out
                BtnCheckout_Click(sender, e);
            }
        }

        private async void BtnUnlock_Click(object sender, EventArgs e)
        {
            if (!_arasService.IsConnected)
            {
                MessageBox.Show("Please connect to Aras first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvResults.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a document from the search results.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string documentId = dgvResults.SelectedRows[0].Cells["ID"].Value?.ToString();
            string itemNumber = dgvResults.SelectedRows[0].Cells["ItemNumber"].Value?.ToString();

            if (string.IsNullOrEmpty(documentId))
            {
                MessageBox.Show("Invalid document selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to unlock document '{itemNumber}'?\n\nThis will release the lock without saving changes.",
                "Confirm Unlock", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            SetStatus("Unlocking document...");
            Log($"Unlock started: {itemNumber}");

            await Task.Run(() =>
            {
                var unlockResult = _arasService.UnlockItem("Document", documentId, out string unlockError);

                this.Invoke((Action)(() =>
                {
                    if (unlockResult != null && !unlockResult.isError())
                    {
                        Log($"Document unlocked successfully: {itemNumber}", LogLevel.Success);
                        MessageBox.Show($"Document '{itemNumber}' has been unlocked.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh search results to update locked status
                        BtnDoSearch_Click(sender, e);
                    }
                    else
                    {
                        Log($"Could not unlock document: {unlockError}", LogLevel.Error);
                        MessageBox.Show("Could not unlock document: " + unlockError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    SetStatus("Ready");
                }));
            });
        }

        private void SetStatus(string message)
        {
            lblStatus.Text = message;
        }

        private enum LogLevel { Info, Success, Warning, Error }

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((Action)(() => Log(message, level)));
                return;
            }

            Color color;
            switch (level)
            {
                case LogLevel.Success: color = Color.LightGreen; break;
                case LogLevel.Warning: color = Color.Yellow; break;
                case LogLevel.Error: color = Color.Red; break;
                default: color = Color.White; break;
            }

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionColor = Color.Gray;
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
            txtLog.SelectionColor = color;
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _arasService?.Dispose();
            _catiaService?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
