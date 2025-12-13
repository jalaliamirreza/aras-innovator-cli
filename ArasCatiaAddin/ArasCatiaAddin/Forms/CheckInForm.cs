using System;
using System.Windows.Forms;
using ArasCatiaAddin.Models;

namespace ArasCatiaAddin.Forms
{
    public partial class CheckInForm : Form
    {
        private readonly ConfigManager _configManager;
        private readonly ArasService _arasService;
        private readonly CatiaDocumentInfo _docInfo;

        public string CreatedItemNumber { get; private set; }

        public CheckInForm(ConfigManager configManager, ArasService arasService, CatiaDocumentInfo docInfo)
        {
            _configManager = configManager;
            _arasService = arasService;
            _docInfo = docInfo;
            InitializeComponent();
            LoadDocumentInfo();
        }

        private void LoadDocumentInfo()
        {
            // Pre-fill from CATIA document
            if (!string.IsNullOrEmpty(_docInfo.PartNumber))
            {
                txtItemNumber.Text = _docInfo.PartNumber;
            }
            else if (_configManager.Config.AutoGenerateItemNumber)
            {
                txtItemNumber.Text = _arasService.GenerateItemNumber(_configManager.Config.ItemNumberPrefix);
            }

            txtName.Text = _docInfo.Nomenclature ?? System.IO.Path.GetFileNameWithoutExtension(_docInfo.FileName);
            txtDescription.Text = _docInfo.Definition ?? "";
            txtFilePath.Text = _docInfo.FullPath;

            // Set document type
            cboDocumentType.Items.AddRange(new object[] {
                "3D Model",
                "Drawing",
                "Assembly",
                "Specification",
                "Other"
            });

            string defaultType = _configManager.Config.DefaultDocumentType;
            if (string.IsNullOrEmpty(defaultType))
            {
                defaultType = _docInfo.DocumentType == "CATDrawing" ? "Drawing" :
                              _docInfo.DocumentType == "CATProduct" ? "Assembly" : "3D Model";
            }

            int idx = cboDocumentType.Items.IndexOf(defaultType);
            cboDocumentType.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtItemNumber.Text))
            {
                MessageBox.Show("Item Number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemNumber.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            // Confirm if configured
            if (_configManager.Config.ConfirmBeforeCheckin)
            {
                var result = MessageBox.Show(
                    $"Check in document?\n\nItem Number: {txtItemNumber.Text}\nName: {txtName.Text}\nFile: {txtFilePath.Text}",
                    "Confirm Check-In",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;
            }

            // Perform check-in
            lblStatus.Text = "Creating document...";
            progressBar.Value = 30;
            btnOK.Enabled = false;
            Application.DoEvents();

            var arasDoc = new ArasDocument
            {
                ItemNumber = txtItemNumber.Text.Trim(),
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                DocumentType = cboDocumentType.SelectedItem?.ToString() ?? "3D Model"
            };

            string docId = _arasService.CreateDocument(arasDoc, out string createError);

            if (docId == null)
            {
                lblStatus.Text = createError ?? "Failed to create document.";
                btnOK.Enabled = true;
                progressBar.Value = 0;
                return;
            }

            // Upload file
            lblStatus.Text = "Uploading file...";
            progressBar.Value = 70;
            Application.DoEvents();

            if (!_arasService.UploadFile(docId, _docInfo.FullPath, out string uploadError))
            {
                lblStatus.Text = uploadError ?? "Failed to upload file.";
                btnOK.Enabled = true;
                progressBar.Value = 0;
                return;
            }

            progressBar.Value = 100;
            CreatedItemNumber = txtItemNumber.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
