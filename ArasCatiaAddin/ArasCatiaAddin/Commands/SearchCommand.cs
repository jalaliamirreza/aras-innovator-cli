using System.Windows.Forms;
using ArasCatiaAddin.Forms;

namespace ArasCatiaAddin.Commands
{
    /// <summary>
    /// Command to handle search workflow.
    /// </summary>
    public class SearchCommand
    {
        private readonly Connect _connect;

        public SearchCommand(Connect connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Execute the search command.
        /// </summary>
        public void Execute()
        {
            Logger.Debug("SearchCommand executing...");

            // Check if logged in
            if (!_connect.ArasService.IsConnected)
            {
                MessageBox.Show(
                    "You must be logged in to Aras to search documents.",
                    "Search",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Show search form
            using (var searchForm = new SearchForm(
                _connect.ConfigManager,
                _connect.ArasService,
                _connect.CatiaService,
                SearchMode.Browse))
            {
                searchForm.ShowDialog();
            }
        }
    }
}
