using System;
using System.Drawing;
using System.Windows.Forms;
using Aras.IOM;
using ArasCLI.Services;

namespace ArasCLI
{
    public class CatiaLoginForm : Form
    {
        // Controls
        private Label lblTitle;
        private Label lblServerUrl;
        private TextBox txtServerUrl;
        private Label lblDatabase;
        private TextBox txtDatabase;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private CheckBox chkRemember;
        private Button btnLogin;
        private Button btnLogout;
        private Button btnCancel;
        private Label lblStatus;
        private Panel statusPanel;

        // Services
        private ConfigManager _configManager;

        // Properties
        public bool IsLoggedIn => SessionManager.HasActiveSession;
        public Innovator ArasInnovator => SessionManager.Innovator;
        public HttpServerConnection Connection => SessionManager.Connection;

        public CatiaLoginForm()
        {
            _configManager = new ConfigManager();
            InitializeComponents();
            LoadSavedCredentials();
            CheckExistingSession();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Aras Innovator - CATIA Login";
            this.Size = new Size(400, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            int yPos = 20;
            int labelWidth = 100;
            int inputWidth = 250;
            int leftMargin = 20;
            int inputLeft = leftMargin + labelWidth + 10;

            // Title
            lblTitle = new Label
            {
                Text = "Aras Innovator Login",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(leftMargin, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);
            yPos += 45;

            // Server URL
            lblServerUrl = new Label
            {
                Text = "Server URL:",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 20)
            };
            txtServerUrl = new TextBox
            {
                Location = new Point(inputLeft, yPos),
                Size = new Size(inputWidth, 25)
            };
            this.Controls.Add(lblServerUrl);
            this.Controls.Add(txtServerUrl);
            yPos += 35;

            // Database
            lblDatabase = new Label
            {
                Text = "Database:",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 20)
            };
            txtDatabase = new TextBox
            {
                Location = new Point(inputLeft, yPos),
                Size = new Size(inputWidth, 25)
            };
            this.Controls.Add(lblDatabase);
            this.Controls.Add(txtDatabase);
            yPos += 35;

            // Username
            lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 20)
            };
            txtUsername = new TextBox
            {
                Location = new Point(inputLeft, yPos),
                Size = new Size(inputWidth, 25)
            };
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            yPos += 35;

            // Password
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 20)
            };
            txtPassword = new TextBox
            {
                Location = new Point(inputLeft, yPos),
                Size = new Size(inputWidth, 25),
                UseSystemPasswordChar = true
            };
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            yPos += 35;

            // Remember checkbox
            chkRemember = new CheckBox
            {
                Text = "Remember credentials",
                Location = new Point(inputLeft, yPos),
                Size = new Size(inputWidth, 25)
            };
            this.Controls.Add(chkRemember);
            yPos += 35;

            // Buttons
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(inputLeft, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnLogout = new Button
            {
                Text = "Logout",
                Location = new Point(inputLeft, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;

            btnCancel = new Button
            {
                Text = "Close",
                Location = new Point(inputLeft + 110, yPos),
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.Add(btnLogin);
            this.Controls.Add(btnLogout);
            this.Controls.Add(btnCancel);
            yPos += 50;

            // Status panel
            statusPanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(400, 60),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            lblStatus = new Label
            {
                Text = "Not connected",
                Location = new Point(leftMargin, 10),
                Size = new Size(350, 40),
                ForeColor = Color.Gray
            };
            statusPanel.Controls.Add(lblStatus);
            this.Controls.Add(statusPanel);

            // Accept button (Enter key)
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }

        private void LoadSavedCredentials()
        {
            var config = _configManager.Config;
            txtServerUrl.Text = config.ArasServerUrl;
            txtDatabase.Text = config.ArasDatabase;
            txtUsername.Text = config.ArasUsername;
            chkRemember.Checked = config.RememberCredentials;
        }

        private void SaveCredentials()
        {
            var config = _configManager.Config;
            config.ArasServerUrl = txtServerUrl.Text.Trim();
            config.ArasDatabase = txtDatabase.Text.Trim();
            config.ArasUsername = txtUsername.Text.Trim();
            config.RememberCredentials = chkRemember.Checked;
            _configManager.Save();
        }

        private void CheckExistingSession()
        {
            // Check if we have an active session via SessionManager
            var sessionInfo = SessionManager.GetSessionInfo();

            if (sessionInfo.HasSavedSession)
            {
                // Pre-fill from saved session
                if (!string.IsNullOrEmpty(sessionInfo.ServerUrl))
                    txtServerUrl.Text = sessionInfo.ServerUrl;
                if (!string.IsNullOrEmpty(sessionInfo.Database))
                    txtDatabase.Text = sessionInfo.Database;
                if (!string.IsNullOrEmpty(sessionInfo.Username))
                    txtUsername.Text = sessionInfo.Username;
            }

            // Try to restore session automatically
            if (SessionManager.HasActiveSession)
            {
                ShowLoggedInState();
                return;
            }

            if (sessionInfo.HasSavedSession && !sessionInfo.SessionExpired)
            {
                SetStatus("Session found. Reconnecting...", Color.Orange);
                ShowLoggedOutState();
            }
            else if (sessionInfo.SessionExpired)
            {
                SetStatus("Session expired. Please login again.", Color.Orange);
                ShowLoggedOutState();
            }
            else
            {
                ShowLoggedOutState();
            }
        }

        private void ShowLoggedInState()
        {
            txtServerUrl.Enabled = false;
            txtDatabase.Enabled = false;
            txtUsername.Enabled = false;
            txtPassword.Enabled = false;
            chkRemember.Enabled = false;

            btnLogin.Visible = false;
            btnLogout.Visible = true;

            var sessionInfo = SessionManager.GetSessionInfo();
            string durationText = sessionInfo.SessionDuration;

            SetStatus($"Connected as: {sessionInfo.Username}\nSession duration: {durationText}", Color.Green);
        }

        private void ShowLoggedOutState()
        {
            txtServerUrl.Enabled = true;
            txtDatabase.Enabled = true;
            txtUsername.Enabled = true;
            txtPassword.Enabled = true;
            chkRemember.Enabled = true;

            btnLogin.Visible = true;
            btnLogout.Visible = false;

            if (string.IsNullOrEmpty(lblStatus.Text) || lblStatus.ForeColor == Color.Green)
            {
                SetStatus("Not connected", Color.Gray);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtServerUrl.Text))
            {
                ShowError("Please enter the server URL.");
                txtServerUrl.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                ShowError("Please enter the database name.");
                txtDatabase.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return;
            }

            // Disable form during login
            SetFormEnabled(false);
            SetStatus("Connecting...", Color.Orange);
            Application.DoEvents();

            try
            {
                // Use SessionManager to login
                var result = SessionManager.Login(
                    txtServerUrl.Text.Trim(),
                    txtDatabase.Text.Trim(),
                    txtUsername.Text.Trim(),
                    txtPassword.Text
                );

                if (!result.Success)
                {
                    SetStatus("Login failed", Color.Red);
                    ShowError("Login failed:\n\n" + result.ErrorMessage);
                    SetFormEnabled(true);
                    ShowLoggedOutState();
                    return;
                }

                // Success - save credentials if requested
                if (chkRemember.Checked)
                {
                    SaveCredentials();
                }

                // Clear password from form
                txtPassword.Text = "";

                // Show success
                ShowLoggedInState();
            }
            catch (Exception ex)
            {
                SetStatus("Connection error", Color.Red);
                ShowError("Connection error:\n\n" + ex.Message);
                SetFormEnabled(true);
                ShowLoggedOutState();
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            SessionManager.Logout();
            ShowLoggedOutState();
            SetStatus("Logged out successfully", Color.Gray);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = IsLoggedIn ? DialogResult.OK : DialogResult.Cancel;
            this.Close();
        }

        private void SetFormEnabled(bool enabled)
        {
            txtServerUrl.Enabled = enabled;
            txtDatabase.Enabled = enabled;
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            chkRemember.Enabled = enabled;
            btnLogin.Enabled = enabled;
        }

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Don't logout when closing - keep session alive
            base.OnFormClosing(e);
        }
    }
}
