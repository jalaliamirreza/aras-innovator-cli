using System.Windows.Forms;
using ArasCatiaAddin.Forms;

namespace ArasCatiaAddin.Commands
{
    /// <summary>
    /// Command to handle get latest workflow.
    /// </summary>
    public class GetLatestCommand
    {
        private readonly Connect _connect;

        public GetLatestCommand(Connect connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Execute the get latest command.
        /// </summary>
        public void Execute()
        {
            Logger.Debug("GetLatestCommand executing...");

            // Check if logged in
            if (!_connect.ArasService.IsConnected)
            {
                MessageBox.Show(
                    "You must be logged in to Aras to get documents.",
                    "Get Latest",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Show search form in get latest mode
            using (var searchForm = new SearchForm(
                _connect.ConfigManager,
                _connect.ArasService,
                _connect.CatiaService,
                SearchMode.GetLatest))
            {
                searchForm.ShowDialog();
            }
        }
    }
}
