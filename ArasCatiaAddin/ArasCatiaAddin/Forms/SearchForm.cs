using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ArasCatiaAddin.Models;

namespace ArasCatiaAddin.Forms
{
    public enum SearchMode
    {
        Browse,
        CheckOut,
        GetLatest
    }

    public partial class SearchForm : Form
    {
        private readonly ConfigManager _configManager;
        private readonly ArasService _arasService;
        private readonly CatiaService _catiaService;
        private readonly SearchMode _mode;
        private List<ArasDocument> _searchResults;

        public ArasDocument SelectedDocument { get; private set; }

        public SearchForm(ConfigManager configManager, ArasService arasService, CatiaService catiaService, SearchMode mode)
        {
            _configManager = configManager;
            _arasService = arasService;
            _catiaService = catiaService;
            _mode = mode;
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Set title based on mode
            switch (_mode)
            {
                case SearchMode.CheckOut:
                    Text = "Check Out Document";
                    btnAction.Text = "Check Out";
                    break;
                case SearchMode.GetLatest:
                    Text = "Get Latest Document";
                    btnAction.Text = "Get Latest";
                    break;
                default:
                    Text = "Search Documents";
                    btnAction.Text = "Open";
                    break;
            }

            // Setup document type dropdown
            cboDocumentType.Items.Add("(Any)");
            cboDocumentType.Items.Add("3D Model");
            cboDocumentType.Items.Add("Drawing");
            cboDocumentType.Items.Add("Assembly");
            cboDocumentType.Items.Add("Specification");
            cboDocumentType.SelectedIndex = 0;

            // Setup grid columns
            dgvResults.AutoGenerateColumns = false;
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemNumber", HeaderText = "Item Number", DataPropertyName = "ItemNumber", Width = 120 });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", DataPropertyName = "Name", Width = 150 });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "DocumentType", HeaderText = "Type", DataPropertyName = "DocumentType", Width = 80 });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "State", HeaderText = "State", DataPropertyName = "State", Width = 80 });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "LockedByName", HeaderText = "Locked By", DataPropertyName = "LockedByName", Width = 100 });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "ModifiedOn", HeaderText = "Modified", DataPropertyName = "ModifiedOn", Width = 120 });
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void Search()
        {
            string itemNumber = txtItemNumber.Text.Trim();
            string name = txtName.Text.Trim();
            string docType = cboDocumentType.SelectedIndex <= 0 ? null : cboDocumentType.SelectedItem.ToString();

            lblStatus.Text = "Searching...";
            btnSearch.Enabled = false;
            Application.DoEvents();

            _searchResults = _arasService.SearchDocuments(itemNumber, name, docType, out string errorMessage);

            if (errorMessage != null)
            {
                lblStatus.Text = errorMessage;
                btnSearch.Enabled = true;
                return;
            }

            dgvResults.DataSource = _searchResults;
            lblStatus.Text = $"Found {_searchResults.Count} document(s).";
            btnSearch.Enabled = true;
        }

        private void btnAction_Click(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a document.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dgvResults.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _searchResults.Count)
                return;

            SelectedDocument = _searchResults[rowIndex];

            switch (_mode)
            {
                case SearchMode.CheckOut:
                    PerformCheckOut();
                    break;
                case SearchMode.GetLatest:
                    PerformGetLatest();
                    break;
                default:
                    // Just select and close
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
            }
        }

        private void PerformCheckOut()
        {
            // Check if already locked
            if (!string.IsNullOrEmpty(SelectedDocument.LockedByName))
            {
                MessageBox.Show(
                    $"Document is already checked out by {SelectedDocument.LockedByName}.",
                    "Check Out",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Text = "Locking document...";
            Application.DoEvents();

            // Lock the document
            if (!_arasService.LockDocument(SelectedDocument.Id, out string lockError))
            {
                MessageBox.Show(
                    $"Failed to lock document: {lockError}",
                    "Check Out",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = lockError;
                return;
            }

            // Download file
            PerformDownloadAndOpen();
        }

        private void PerformGetLatest()
        {
            PerformDownloadAndOpen();
        }

        private void PerformDownloadAndOpen()
        {
            lblStatus.Text = "Downloading file...";
            Application.DoEvents();

            _configManager.EnsureWorkspaceExists();
            string localPath = _configManager.Config.LocalWorkspace;

            if (!_arasService.DownloadFile(SelectedDocument.Id, localPath, out string downloadError))
            {
                MessageBox.Show(
                    $"Failed to download file: {downloadError}",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = downloadError;
                return;
            }

            // Open in CATIA
            string filePath = System.IO.Path.Combine(localPath, SelectedDocument.ItemNumber + ".CATPart"); // Simplified - need to get actual filename

            lblStatus.Text = "Opening in CATIA...";
            Application.DoEvents();

            if (_catiaService.OpenDocument(filePath, out string openError))
            {
                MessageBox.Show(
                    "Document downloaded and opened successfully.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(
                    $"File downloaded but could not open in CATIA: {openError}",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAction_Click(sender, e);
            }
        }

        private void txtItemNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
                e.Handled = true;
            }
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
                e.Handled = true;
            }
        }
    }
}
