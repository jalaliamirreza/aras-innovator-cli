using System;
using System.Windows.Forms;

namespace ArasCatiaAddin.Forms
{
    public partial class LoginForm : Form
    {
        private readonly ConfigManager _configManager;
        private readonly ArasService _arasService;

        public LoginForm(ConfigManager configManager, ArasService arasService)
        {
            _configManager = configManager;
            _arasService = arasService;
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            txtServerUrl.Text = _configManager.Config.ArasServerUrl;
            txtDatabase.Text = _configManager.Config.ArasDatabase;
            txtUsername.Text = _configManager.Config.ArasUsername;
            chkRememberPassword.Checked = _configManager.Config.RememberPassword;

            if (_configManager.Config.RememberPassword)
            {
                txtPassword.Text = _configManager.Config.ArasPassword;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtServerUrl.Text))
            {
                ShowError("Server URL is required.");
                txtServerUrl.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                ShowError("Database is required.");
                txtDatabase.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Username is required.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowError("Password is required.");
                txtPassword.Focus();
                return;
            }

            // Attempt login
            lblStatus.Text = "Connecting...";
            lblStatus.ForeColor = System.Drawing.Color.Blue;
            btnOK.Enabled = false;
            Application.DoEvents();

            if (_arasService.Connect(
                txtServerUrl.Text.Trim(),
                txtDatabase.Text.Trim(),
                txtUsername.Text.Trim(),
                txtPassword.Text,
                out string errorMessage))
            {
                // Save settings
                _configManager.Config.ArasServerUrl = txtServerUrl.Text.Trim();
                _configManager.Config.ArasDatabase = txtDatabase.Text.Trim();
                _configManager.Config.ArasUsername = txtUsername.Text.Trim();
                _configManager.Config.RememberPassword = chkRememberPassword.Checked;

                if (chkRememberPassword.Checked)
                {
                    _configManager.Config.ArasPassword = txtPassword.Text;
                }
                else
                {
                    _configManager.Config.ArasPassword = null;
                }

                _configManager.SaveConfig();

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblStatus.Text = errorMessage ?? "Login failed.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                btnOK.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ShowError(string message)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }
    }
}
