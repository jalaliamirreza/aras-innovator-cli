using System.Windows.Forms;
using ArasCatiaAddin.Forms;

namespace ArasCatiaAddin.Commands
{
    /// <summary>
    /// Command to handle check-in workflow.
    /// </summary>
    public class CheckInCommand
    {
        private readonly Connect _connect;

        public CheckInCommand(Connect connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Execute the check-in command.
        /// </summary>
        public void Execute()
        {
            Logger.Debug("CheckInCommand executing...");

            // Check if logged in
            if (!_connect.ArasService.IsConnected)
            {
                MessageBox.Show(
                    "You must be logged in to Aras to check in documents.",
                    "Check In",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Get active document from CATIA
            var docInfo = _connect.CatiaService.GetActiveDocument(out string errorMessage);

            if (docInfo == null)
            {
                MessageBox.Show(
                    errorMessage ?? "No document is open in CATIA.",
                    "Check In",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Check if document is saved
            if (!docInfo.IsSaved)
            {
                var saveResult = MessageBox.Show(
                    "The document has unsaved changes. Do you want to save before check-in?",
                    "Check In",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (saveResult == DialogResult.Cancel)
                    return;

                if (saveResult == DialogResult.Yes)
                {
                    if (!_connect.CatiaService.SaveActiveDocument(out string saveError))
                    {
                        MessageBox.Show(
                            $"Failed to save document: {saveError}",
                            "Check In",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            // Show check-in form
            using (var checkInForm = new CheckInForm(
                _connect.ConfigManager,
                _connect.ArasService,
                docInfo))
            {
                if (checkInForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show(
                        $"Document checked in successfully.\nItem Number: {checkInForm.CreatedItemNumber}",
                        "Check In",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }
}
