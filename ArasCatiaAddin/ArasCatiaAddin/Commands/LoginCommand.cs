using System.Windows.Forms;
using ArasCatiaAddin.Forms;

namespace ArasCatiaAddin.Commands
{
    /// <summary>
    /// Command to handle login workflow.
    /// </summary>
    public class LoginCommand
    {
        private readonly Connect _connect;

        public LoginCommand(Connect connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Execute the login command.
        /// </summary>
        public void Execute()
        {
            Logger.Debug("LoginCommand executing...");

            // If already connected, ask if user wants to logout
            if (_connect.ArasService.IsConnected)
            {
                var result = MessageBox.Show(
                    $"You are already logged in as {_connect.ArasService.CurrentUser}.\n\nDo you want to log out?",
                    "Aras Login",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _connect.ArasService.Disconnect();
                    _connect.UpdateToolbarState();
                    MessageBox.Show("Logged out successfully.", "Aras Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            // Show login form
            using (var loginForm = new LoginForm(_connect.ConfigManager, _connect.ArasService))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    _connect.UpdateToolbarState();
                    MessageBox.Show(
                        $"Logged in as {_connect.ArasService.CurrentUser}",
                        "Aras Login",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }
}
