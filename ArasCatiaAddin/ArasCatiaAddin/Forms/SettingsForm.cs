using System;
using System.Windows.Forms;

namespace ArasCatiaAddin.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigManager _configManager;

        public SettingsForm(ConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var config = _configManager.Config;

            // Connection
            txtServerUrl.Text = config.ArasServerUrl;
            txtDatabase.Text = config.ArasDatabase;
            chkRememberPassword.Checked = config.RememberPassword;
            chkAutoLogin.Checked = config.AutoLogin;

            // Workspace
            txtWorkspace.Text = config.LocalWorkspace;
            chkOverwrite.Checked = config.OverwriteExisting;

            // Check-in
            chkAutoGenerate.Checked = config.AutoGenerateItemNumber;
            txtPrefix.Text = config.ItemNumberPrefix;
            cboDefaultType.Items.AddRange(new object[] {
                "3D Model", "Drawing", "Assembly", "Specification", "Other"
            });
            int idx = cboDefaultType.Items.IndexOf(config.DefaultDocumentType);
            cboDefaultType.SelectedIndex = idx >= 0 ? idx : 0;
            chkConfirmCheckin.Checked = config.ConfirmBeforeCheckin;

            // BOM
            chkCreateMissingParts.Checked = config.CreateMissingParts;
            chkSyncProperties.Checked = config.SyncProperties;
        }

        private void SaveSettings()
        {
            var config = _configManager.Config;

            // Connection
            config.ArasServerUrl = txtServerUrl.Text.Trim();
            config.ArasDatabase = txtDatabase.Text.Trim();
            config.RememberPassword = chkRememberPassword.Checked;
            config.AutoLogin = chkAutoLogin.Checked;

            // Workspace
            config.LocalWorkspace = txtWorkspace.Text.Trim();
            config.OverwriteExisting = chkOverwrite.Checked;

            // Check-in
            config.AutoGenerateItemNumber = chkAutoGenerate.Checked;
            config.ItemNumberPrefix = txtPrefix.Text.Trim();
            config.DefaultDocumentType = cboDefaultType.SelectedItem?.ToString() ?? "3D Model";
            config.ConfirmBeforeCheckin = chkConfirmCheckin.Checked;

            // BOM
            config.CreateMissingParts = chkCreateMissingParts.Checked;
            config.SyncProperties = chkSyncProperties.Checked;

            _configManager.SaveConfig();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
            MessageBox.Show("Settings saved.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Reset all settings to defaults?",
                "Reset Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _configManager.ResetConfig();
                LoadSettings();
            }
        }

        private void btnBrowseWorkspace_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select workspace folder";
                folderDialog.SelectedPath = txtWorkspace.Text;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtWorkspace.Text = folderDialog.SelectedPath;
                }
            }
        }
    }
}
