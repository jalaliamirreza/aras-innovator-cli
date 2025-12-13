using System.Windows.Forms;
using ArasCatiaAddin.Forms;

namespace ArasCatiaAddin.Commands
{
    /// <summary>
    /// Command to handle check-out workflow.
    /// </summary>
    public class CheckOutCommand
    {
        private readonly Connect _connect;

        public CheckOutCommand(Connect connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Execute the check-out command.
        /// </summary>
        public void Execute()
        {
            Logger.Debug("CheckOutCommand executing...");

            // Check if logged in
            if (!_connect.ArasService.IsConnected)
            {
                MessageBox.Show(
                    "You must be logged in to Aras to check out documents.",
                    "Check Out",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Show search form in check-out mode
            using (var searchForm = new SearchForm(
                _connect.ConfigManager,
                _connect.ArasService,
                _connect.CatiaService,
                SearchMode.CheckOut))
            {
                searchForm.ShowDialog();
            }
        }
    }
}
