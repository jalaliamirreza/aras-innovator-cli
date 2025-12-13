using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ArasCatiaAddin.Models;

namespace ArasCatiaAddin.Forms
{
    public partial class BomSyncForm : Form
    {
        private readonly ConfigManager _configManager;
        private readonly ArasService _arasService;
        private readonly List<BomItem> _bomItems;

        public BomSyncForm(ConfigManager configManager, ArasService arasService, List<BomItem> bomItems)
        {
            _configManager = configManager;
            _arasService = arasService;
            _bomItems = bomItems;
            InitializeComponent();
            LoadBomTree();
        }

        private void LoadBomTree()
        {
            tvBom.Nodes.Clear();

            foreach (var item in _bomItems)
            {
                TreeNode node = CreateNode(item);
                tvBom.Nodes.Add(node);
            }

            tvBom.ExpandAll();
            lblStatus.Text = $"Found {_bomItems.Count} BOM item(s).";
        }

        private TreeNode CreateNode(BomItem item)
        {
            string text = $"{item.PartNumber} - {item.Name}";
            TreeNode node = new TreeNode(text);
            node.Tag = item;
            node.Checked = true;

            // Set icon based on status
            switch (item.SyncStatus)
            {
                case SyncStatus.New:
                    node.ForeColor = System.Drawing.Color.Green;
                    break;
                case SyncStatus.Exists:
                    node.ForeColor = System.Drawing.Color.Gray;
                    break;
                case SyncStatus.Modified:
                    node.ForeColor = System.Drawing.Color.Orange;
                    break;
                case SyncStatus.Error:
                    node.ForeColor = System.Drawing.Color.Red;
                    break;
            }

            // Add children
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    node.Nodes.Add(CreateNode(child));
                }
            }

            return node;
        }

        private void tvBom_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is BomItem item)
            {
                txtPartNumber.Text = item.PartNumber;
                txtPartName.Text = item.Name;
                txtNomenclature.Text = item.Nomenclature;
                txtRevision.Text = item.Revision;
                txtFilePath.Text = item.FilePath;
                txtFileType.Text = item.FileType;
                txtStatus.Text = item.SyncStatus.ToString();
            }
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (TreeNode node in tvBom.Nodes)
            {
                SetNodeChecked(node, chkSelectAll.Checked);
            }
        }

        private void SetNodeChecked(TreeNode node, bool isChecked)
        {
            node.Checked = isChecked;
            foreach (TreeNode child in node.Nodes)
            {
                SetNodeChecked(child, isChecked);
            }
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            // Get selected items
            var selectedItems = new List<BomItem>();
            CollectSelectedItems(tvBom.Nodes, selectedItems);

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one item to sync.", "BOM Sync",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Sync {selectedItems.Count} item(s) to Aras?",
                "Confirm BOM Sync",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Perform sync
            btnSync.Enabled = false;
            progressBar.Maximum = selectedItems.Count;
            progressBar.Value = 0;
            txtLog.Clear();

            int successCount = 0;
            int errorCount = 0;

            foreach (var item in selectedItems)
            {
                lblStatus.Text = $"Syncing {item.PartNumber}...";
                Application.DoEvents();

                // Create part in Aras
                string partId = _arasService.CreatePart(item, out string error);

                if (partId != null)
                {
                    txtLog.AppendText($"[OK] Created Part: {item.PartNumber}\r\n");
                    item.ArasPartId = partId;
                    item.SyncStatus = SyncStatus.Exists;
                    successCount++;
                }
                else
                {
                    txtLog.AppendText($"[ERROR] {item.PartNumber}: {error}\r\n");
                    item.SyncStatus = SyncStatus.Error;
                    errorCount++;
                }

                progressBar.Value++;
                Application.DoEvents();
            }

            // Refresh tree
            LoadBomTree();

            lblStatus.Text = $"Sync complete. Success: {successCount}, Errors: {errorCount}";
            btnSync.Enabled = true;

            MessageBox.Show(
                $"BOM Sync Complete\n\nSuccess: {successCount}\nErrors: {errorCount}",
                "BOM Sync",
                MessageBoxButtons.OK,
                errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void CollectSelectedItems(TreeNodeCollection nodes, List<BomItem> selectedItems)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is BomItem item)
                {
                    if (item.SyncStatus == SyncStatus.New || item.SyncStatus == SyncStatus.Modified)
                    {
                        selectedItems.Add(item);
                    }
                }
                CollectSelectedItems(node.Nodes, selectedItems);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
