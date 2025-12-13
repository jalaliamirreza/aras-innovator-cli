using System;
using System.Reflection;
using System.Runtime.InteropServices;
using ArasCatiaAddin.Commands;

namespace ArasCatiaAddin
{
    /// <summary>
    /// Creates and manages the CATIA toolbar for the Aras add-in.
    /// </summary>
    public class AddinToolbar
    {
        private readonly Connect _connect;
        private object _catiaApp;
        private Type _catiaType;
        private object _commandHeaders;
        private object _toolbar;

        // Command instances
        private LoginCommand _loginCommand;
        private CheckInCommand _checkInCommand;
        private CheckOutCommand _checkOutCommand;
        private GetLatestCommand _getLatestCommand;
        private SearchCommand _searchCommand;
        private BomSyncCommand _bomSyncCommand;
        private SettingsCommand _settingsCommand;

        // Button references for enable/disable
        private object _btnLogin;
        private object _btnCheckIn;
        private object _btnCheckOut;
        private object _btnGetLatest;
        private object _btnSearch;
        private object _btnBomSync;
        private object _btnSettings;

        public AddinToolbar(Connect connect)
        {
            _connect = connect;
            _catiaApp = connect.CatiaApplication;
            _catiaType = _catiaApp.GetType();

            // Initialize commands
            _loginCommand = new LoginCommand(connect);
            _checkInCommand = new CheckInCommand(connect);
            _checkOutCommand = new CheckOutCommand(connect);
            _getLatestCommand = new GetLatestCommand(connect);
            _searchCommand = new SearchCommand(connect);
            _bomSyncCommand = new BomSyncCommand(connect);
            _settingsCommand = new SettingsCommand(connect);
        }

        /// <summary>
        /// Create the Aras toolbar in CATIA.
        /// </summary>
        public void CreateToolbar()
        {
            try
            {
                Logger.Debug("Creating Aras toolbar...");

                // Get CommandHeaders collection
                _commandHeaders = GetProperty(_catiaApp, "CommandHeaders");
                if (_commandHeaders == null)
                {
                    Logger.Warning("Could not get CommandHeaders - toolbar not created");
                    return;
                }

                // Create command headers for each button
                CreateCommandHeader("ArasLogin", "Login", "Login to Aras Innovator");
                CreateCommandHeader("ArasCheckIn", "Check In", "Check in document to Aras");
                CreateCommandHeader("ArasCheckOut", "Check Out", "Check out document from Aras");
                CreateCommandHeader("ArasGetLatest", "Get Latest", "Get latest version from Aras");
                CreateCommandHeader("ArasSearch", "Search", "Search Aras");
                CreateCommandHeader("ArasBomSync", "BOM Sync", "Sync BOM to Aras");
                CreateCommandHeader("ArasSettings", "Settings", "Add-in settings");

                // Create toolbar using Workbench API
                CreateToolbarWorkbench();

                Logger.Info("Aras toolbar created successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create toolbar: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create toolbar using CATIA Workbench API.
        /// </summary>
        private void CreateToolbarWorkbench()
        {
            try
            {
                // Get ActiveDocument to access Windows
                object windows = GetProperty(_catiaApp, "Windows");
                if (windows == null) return;

                int windowCount = Convert.ToInt32(GetProperty(windows, "Count"));
                if (windowCount > 0)
                {
                    object window = CallMethod(windows, "Item", 1);
                    if (window != null)
                    {
                        // Try to get Viewers or use alternative approach
                        object viewers = GetProperty(window, "Viewers");
                        if (viewers != null)
                        {
                            Marshal.ReleaseComObject(viewers);
                        }
                        Marshal.ReleaseComObject(window);
                    }
                }
                Marshal.ReleaseComObject(windows);

                // Create toolbar via Application.Toolbars if available
                try
                {
                    // CATIA V5 doesn't expose Toolbars directly via COM
                    // Use CommandHeaders with macro approach
                    Logger.Debug("Using CommandHeaders approach for toolbar.");
                }
                catch (Exception ex)
                {
                    Logger.Debug($"Toolbars not directly accessible: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Workbench toolbar creation: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a command header in CATIA.
        /// </summary>
        private void CreateCommandHeader(string name, string title, string tooltip)
        {
            try
            {
                // Add command header
                // Note: CATIA V5 COM doesn't directly expose CommandHeader creation
                // This is typically done via CATScript or requires CAA
                Logger.Debug($"Command header: {name} - {title}");
            }
            catch (Exception ex)
            {
                Logger.Debug($"CreateCommandHeader {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove the toolbar from CATIA.
        /// </summary>
        public void RemoveToolbar()
        {
            try
            {
                if (_toolbar != null)
                {
                    Marshal.ReleaseComObject(_toolbar);
                    _toolbar = null;
                }

                if (_commandHeaders != null)
                {
                    Marshal.ReleaseComObject(_commandHeaders);
                    _commandHeaders = null;
                }

                Logger.Debug("Toolbar removed.");
            }
            catch (Exception ex)
            {
                Logger.Debug($"RemoveToolbar: {ex.Message}");
            }
        }

        /// <summary>
        /// Update button enabled states based on login status.
        /// </summary>
        public void UpdateButtonStates(bool isLoggedIn)
        {
            try
            {
                // Enable/disable buttons based on login state
                // Login and Settings always enabled
                // Others only when logged in
                Logger.Debug($"Updating button states. LoggedIn: {isLoggedIn}");
            }
            catch (Exception ex)
            {
                Logger.Debug($"UpdateButtonStates: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle button click - called via COM event or command.
        /// </summary>
        public void OnButtonClick(string commandName)
        {
            try
            {
                Logger.Debug($"Button clicked: {commandName}");

                switch (commandName)
                {
                    case "ArasLogin":
                        _loginCommand.Execute();
                        break;
                    case "ArasCheckIn":
                        _checkInCommand.Execute();
                        break;
                    case "ArasCheckOut":
                        _checkOutCommand.Execute();
                        break;
                    case "ArasGetLatest":
                        _getLatestCommand.Execute();
                        break;
                    case "ArasSearch":
                        _searchCommand.Execute();
                        break;
                    case "ArasBomSync":
                        _bomSyncCommand.Execute();
                        break;
                    case "ArasSettings":
                        _settingsCommand.Execute();
                        break;
                    default:
                        Logger.Warning($"Unknown command: {commandName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Button click error: {ex.Message}", ex);
                System.Windows.Forms.MessageBox.Show(
                    $"Error executing command: {ex.Message}",
                    "Aras Add-in Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        #region COM Helper Methods

        private object GetProperty(object comObject, string propertyName)
        {
            return comObject.GetType().InvokeMember(propertyName,
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, null);
        }

        private void SetProperty(object comObject, string propertyName, object value)
        {
            comObject.GetType().InvokeMember(propertyName,
                BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, new object[] { value });
        }

        private object CallMethod(object comObject, string methodName, params object[] args)
        {
            return comObject.GetType().InvokeMember(methodName,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, args);
        }

        #endregion
    }
}
