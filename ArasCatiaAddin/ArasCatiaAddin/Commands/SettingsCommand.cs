using System.Windows.Forms;
using ArasCatiaAddin.Forms;

namespace ArasCatiaAddin.Commands
{
    /// <summary>
    /// Command to handle settings workflow.
    /// </summary>
    public class SettingsCommand
    {
        private readonly Connect _connect;

        public SettingsCommand(Connect connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Execute the settings command.
        /// </summary>
        public void Execute()
        {
            Logger.Debug("SettingsCommand executing...");

            // Show settings form
            using (var settingsForm = new SettingsForm(_connect.ConfigManager))
            {
                settingsForm.ShowDialog();
            }
        }
    }
}
