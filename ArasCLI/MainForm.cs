using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Configuration;
using Aras.IOM;

namespace ArasCLI
{
    public partial class MainForm : Form
    {
        private TextBox txtUrl;
        private TextBox txtDatabase;
        private TextBox txtUser;
        private TextBox txtPassword;
        private TextBox txtFilePath;
        private TextBox txtDocumentName;
        private TextBox txtItemNumber;
        private Button btnBrowseFile;
        private Button btnUpload;
        private Button btnSendToSetup;
        private RichTextBox txtLog;
        private Label lblStatus;
        private CheckBox chkSaveCredentials;
        
        private string configFilePath;

        public MainForm()
        {
            InitializeComponent();
            configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArasCLI", "settings.config");
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Aras Innovator CLI - Document Upload";
            this.Size = new System.Drawing.Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            int yPos = 20;
            int labelWidth = 120;
            int textBoxWidth = 400;
            int spacing = 30;

            // URL
            Label lblUrl = new Label { Text = "Aras URL:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(labelWidth, 23) };
            txtUrl = new TextBox { Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(textBoxWidth, 23) };
            txtUrl.Text = "http://localhost/InnovatorServer";
            this.Controls.Add(lblUrl);
            this.Controls.Add(txtUrl);
            yPos += spacing;

            // Database
            Label lblDatabase = new Label { Text = "Database:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(labelWidth, 23) };
            txtDatabase = new TextBox { Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(textBoxWidth, 23) };
            this.Controls.Add(lblDatabase);
            this.Controls.Add(txtDatabase);
            yPos += spacing;

            // User
            Label lblUser = new Label { Text = "User:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(labelWidth, 23) };
            txtUser = new TextBox { Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(textBoxWidth, 23) };
            this.Controls.Add(lblUser);
            this.Controls.Add(txtUser);
            yPos += spacing;

            // Password
            Label lblPassword = new Label { Text = "Password:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(labelWidth, 23) };
            txtPassword = new TextBox { Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(textBoxWidth, 23), UseSystemPasswordChar = true };
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            yPos += spacing;

            // Save credentials checkbox
            chkSaveCredentials = new CheckBox { Text = "Save credentials", Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(200, 23) };
            this.Controls.Add(chkSaveCredentials);
            yPos += spacing + 10;

            // Separator line
            Label separator = new Label { Text = "─────────────────────────────────────────────────────", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(550, 23), ForeColor = System.Drawing.Color.Gray };
            this.Controls.Add(separator);
            yPos += spacing;

            // File path
            Label lblFile = new Label { Text = "File to Upload:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(labelWidth, 23) };
            txtFilePath = new TextBox { Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(textBoxWidth - 100, 23) };
            txtFilePath.AllowDrop = true;
            txtFilePath.DragEnter += TxtFilePath_DragEnter;
            txtFilePath.DragDrop += TxtFilePath_DragDrop;
            btnBrowseFile = new Button { Text = "Browse...", Location = new System.Drawing.Point(450, yPos - 2), Size = new System.Drawing.Size(90, 27) };
            btnBrowseFile.Click += BtnBrowseFile_Click;
            this.Controls.Add(lblFile);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnBrowseFile);
            yPos += spacing;

            // Document Name
            Label lblDocName = new Label { Text = "Document Name:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(labelWidth, 23) };
            txtDocumentName = new TextBox { Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(textBoxWidth, 23) };
            this.Controls.Add(lblDocName);
            this.Controls.Add(txtDocumentName);
            yPos += spacing;

            // Item Number
            Label lblItemNumber = new Label { Text = "Item Number:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(labelWidth, 23) };
            txtItemNumber = new TextBox { Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(textBoxWidth, 23) };
            Label lblItemNumberHint = new Label { Text = "(leave empty to auto-generate)", Location = new System.Drawing.Point(150, yPos + 23), Size = new System.Drawing.Size(300, 20), ForeColor = System.Drawing.Color.Gray, Font = new System.Drawing.Font("Microsoft Sans Serif", 8F) };
            this.Controls.Add(lblItemNumber);
            this.Controls.Add(txtItemNumber);
            this.Controls.Add(lblItemNumberHint);
            yPos += spacing + 20;

            // Buttons
            btnUpload = new Button { Text = "Upload to Aras", Location = new System.Drawing.Point(150, yPos), Size = new System.Drawing.Size(150, 35), BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White, Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold) };
            btnUpload.Click += BtnUpload_Click;
            this.Controls.Add(btnUpload);

            btnSendToSetup = new Button { Text = "Setup Send To...", Location = new System.Drawing.Point(320, yPos), Size = new System.Drawing.Size(150, 35) };
            btnSendToSetup.Click += BtnSendToSetup_Click;
            this.Controls.Add(btnSendToSetup);

            Button btnCatiaIntegration = new Button { Text = "CATIA Integration", Location = new System.Drawing.Point(490, yPos), Size = new System.Drawing.Size(150, 35), BackColor = System.Drawing.Color.FromArgb(0, 150, 136), ForeColor = System.Drawing.Color.White };
            btnCatiaIntegration.Click += BtnCatiaIntegration_Click;
            this.Controls.Add(btnCatiaIntegration);
            yPos += 50;

            // Status label
            lblStatus = new Label { Text = "Ready", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(550, 23), ForeColor = System.Drawing.Color.Blue };
            this.Controls.Add(lblStatus);
            yPos += 30;

            // Log area
            Label lblLog = new Label { Text = "Log:", Location = new System.Drawing.Point(20, yPos), Size = new System.Drawing.Size(100, 23) };
            txtLog = new RichTextBox { Location = new System.Drawing.Point(20, yPos + 25), Size = new System.Drawing.Size(640, 200), ReadOnly = true, Font = new System.Drawing.Font("Consolas", 9F) };
            this.Controls.Add(lblLog);
            this.Controls.Add(txtLog);

            this.ResumeLayout(false);
        }

        private void BtnBrowseFile_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnBrowseFile.Enabled = false;
                Application.DoEvents();
                
                OpenFileDialog dialog = new OpenFileDialog();
                try
                {
                    dialog.Filter = "All Files (*.*)|*.*";
                    dialog.Title = "Select file to upload";
                    dialog.CheckFileExists = true;
                    dialog.CheckPathExists = true;
                    dialog.Multiselect = false;
                    dialog.RestoreDirectory = true;
                    
                    // Ensure form is visible and active
                    if (!this.Visible)
                    {
                        this.Show();
                    }
                    if (!this.Focused)
                    {
                        this.BringToFront();
                        this.Activate();
                    }
                    
                    DialogResult result = dialog.ShowDialog(this);
                    
                    if (result == DialogResult.OK && !string.IsNullOrEmpty(dialog.FileName))
                    {
                        txtFilePath.Text = dialog.FileName;
                        if (string.IsNullOrEmpty(txtDocumentName.Text))
                        {
                            txtDocumentName.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                        }
                        LogMessage("File selected: " + Path.GetFileName(dialog.FileName));
                    }
                }
                finally
                {
                    dialog.Dispose();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Error opening file dialog: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += Environment.NewLine + "Inner: " + ex.InnerException.Message;
                }
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage("ERROR: " + errorMsg);
            }
            finally
            {
                btnBrowseFile.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void TxtFilePath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TxtFilePath_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && File.Exists(files[0]))
                {
                    txtFilePath.Text = files[0];
                    if (string.IsNullOrEmpty(txtDocumentName.Text))
                    {
                        txtDocumentName.Text = Path.GetFileNameWithoutExtension(files[0]);
                    }
                    LogMessage("File dropped: " + Path.GetFileName(files[0]));
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR dropping file: " + ex.Message);
            }
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(txtUrl.Text) || string.IsNullOrEmpty(txtDatabase.Text) || 
                string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Please fill in all connection settings.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Please select a valid file to upload.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable button during upload
            btnUpload.Enabled = false;
            lblStatus.Text = "Connecting...";
            lblStatus.ForeColor = System.Drawing.Color.Orange;
            txtLog.Clear();
            Application.DoEvents();

            // Save settings if requested
            if (chkSaveCredentials.Checked)
            {
                SaveSettings();
            }

            // Run upload in background
            System.Threading.Tasks.Task.Run(() => PerformUpload());
        }

        private void PerformUpload()
        {
            try
            {
                LogMessage("Connecting to Aras Innovator...");
                HttpServerConnection conn = IomFactory.CreateHttpServerConnection(txtUrl.Text, txtDatabase.Text, txtUser.Text, txtPassword.Text);
                Item loginResult = conn.Login();

                if (loginResult.isError())
                {
                    this.Invoke(new Action(() =>
                    {
                        LogMessage("ERROR: " + loginResult.getErrorString());
                        lblStatus.Text = "Connection failed";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        btnUpload.Enabled = true;
                    }));
                    return;
                }

                this.Invoke(new Action(() => { LogMessage("Connected successfully."); }));

                Innovator inn = IomFactory.CreateInnovator(conn);
                Item result = Program.CreateDocumentAndCheckinFile(inn, conn, txtFilePath.Text, txtItemNumber.Text, txtDocumentName.Text);

                if (result.isError())
                {
                    this.Invoke(new Action(() =>
                    {
                        LogMessage("ERROR: " + result.getErrorString());
                        lblStatus.Text = "Upload failed";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        btnUpload.Enabled = true;
                    }));
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        LogMessage("SUCCESS: Document created and file uploaded!");
                        LogMessage("Document ID: " + result.getID());
                        lblStatus.Text = "Upload successful!";
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                        btnUpload.Enabled = true;
                    }));
                }

                conn.Logout();
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    LogMessage("EXCEPTION: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        LogMessage("Inner: " + ex.InnerException.Message);
                    }
                    lblStatus.Text = "Error occurred";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    btnUpload.Enabled = true;
                }));
            }
        }

        private void LogMessage(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => { txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + message + Environment.NewLine); }));
            }
            else
            {
                txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + message + Environment.NewLine);
            }
        }

        private void BtnSendToSetup_Click(object sender, EventArgs e)
        {
            try
            {
                string sendToPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), "Aras Innovator Upload.lnk");
                string exePath = Application.ExecutablePath;

                // Create shortcut using Windows Script Host
                // Note: Windows Send To passes file path as argument automatically
                string vbsScript = $@"
Set oWS = WScript.CreateObject(""WScript.Shell"")
sLinkFile = ""{sendToPath.Replace("\\", "\\\\")}""
Set oLink = oWS.CreateShortcut(sLinkFile)
oLink.TargetPath = ""{exePath.Replace("\\", "\\\\")}""
oLink.Arguments = ""--gui""
oLink.WorkingDirectory = ""{Path.GetDirectoryName(exePath).Replace("\\", "\\\\")}""
oLink.Description = ""Upload file to Aras Innovator""
oLink.Save
";

                string tempVbs = Path.Combine(Path.GetTempPath(), "create_shortcut.vbs");
                File.WriteAllText(tempVbs, vbsScript);

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cscript.exe",
                    Arguments = $"//nologo \"{tempVbs}\"",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(psi).WaitForExit();

                File.Delete(tempVbs);

                MessageBox.Show($"Send To shortcut created successfully!\n\nYou can now right-click any file and select 'Send to > Aras Innovator Upload' to upload it.", 
                    "Send To Setup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating Send To shortcut: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
                var config = new System.Collections.Specialized.NameValueCollection();
                config["Url"] = txtUrl.Text;
                config["Database"] = txtDatabase.Text;
                config["User"] = txtUser.Text;
                // Note: Password is not saved for security reasons
                
                var configString = new StringBuilder();
                foreach (string key in config.AllKeys)
                {
                    configString.AppendLine($"{key}={config[key]}");
                }
                File.WriteAllText(configFilePath, configString.ToString());
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    var lines = File.ReadAllLines(configFilePath);
                    foreach (var line in lines)
                    {
                        if (line.Contains("="))
                        {
                            var parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                switch (parts[0])
                                {
                                    case "Url":
                                        txtUrl.Text = parts[1];
                                        break;
                                    case "Database":
                                        txtDatabase.Text = parts[1];
                                        break;
                                    case "User":
                                        txtUser.Text = parts[1];
                                        break;
                                }
                            }
                        }
                    }
                    chkSaveCredentials.Checked = true;
                }
            }
            catch { }
        }

        public void SetFileFromSendTo(string filePath)
        {
            if (File.Exists(filePath))
            {
                txtFilePath.Text = filePath;
                if (string.IsNullOrEmpty(txtDocumentName.Text))
                {
                    txtDocumentName.Text = Path.GetFileNameWithoutExtension(filePath);
                }
            }
        }

        private void BtnCatiaIntegration_Click(object sender, EventArgs e)
        {
            CatiaIntegrationForm catiaForm = new CatiaIntegrationForm();
            catiaForm.Show();
        }
    }
}

