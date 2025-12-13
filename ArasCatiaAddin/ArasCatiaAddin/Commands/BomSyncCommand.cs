using System.Windows.Forms;
using ArasCatiaAddin.Forms;

namespace ArasCatiaAddin.Commands
{
    /// <summary>
    /// Command to handle BOM sync workflow.
    /// </summary>
    public class BomSyncCommand
    {
        private readonly Connect _connect;

        public BomSyncCommand(Connect connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Execute the BOM sync command.
        /// </summary>
        public void Execute()
        {
            Logger.Debug("BomSyncCommand executing...");

            // Check if logged in
            if (!_connect.ArasService.IsConnected)
            {
                MessageBox.Show(
                    "You must be logged in to Aras to sync BOM.",
                    "BOM Sync",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Get active document and check if it's a CATProduct
            var docInfo = _connect.CatiaService.GetActiveDocument(out string errorMessage);

            if (docInfo == null)
            {
                MessageBox.Show(
                    errorMessage ?? "No document is open in CATIA.",
                    "BOM Sync",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (docInfo.DocumentType != "CATProduct")
            {
                MessageBox.Show(
                    "BOM Sync requires a CATProduct (assembly) to be open in CATIA.",
                    "BOM Sync",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Get BOM from CATIA
            var bomItems = _connect.CatiaService.GetBom(out string bomError);

            if (bomError != null)
            {
                MessageBox.Show(
                    $"Error extracting BOM: {bomError}",
                    "BOM Sync",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (bomItems.Count == 0)
            {
                MessageBox.Show(
                    "The assembly is empty (no components found).",
                    "BOM Sync",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Show BOM sync form
            using (var bomSyncForm = new BomSyncForm(
                _connect.ConfigManager,
                _connect.ArasService,
                bomItems))
            {
                bomSyncForm.ShowDialog();
            }
        }
    }
}
