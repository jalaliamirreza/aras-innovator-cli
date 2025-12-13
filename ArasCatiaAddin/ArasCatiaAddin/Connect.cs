using System;
using System.Runtime.InteropServices;
using Extensibility;

namespace ArasCatiaAddin
{
    /// <summary>
    /// Main entry point for the CATIA COM Add-in.
    /// Implements IDTExtensibility2 for add-in lifecycle management.
    /// </summary>
    [ComVisible(true)]
    [Guid("F1E2D3C4-B5A6-7890-1234-567890ABCDEF")]
    [ProgId("ArasCatiaAddin.Connect")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Connect : IDTExtensibility2
    {
        private object _catiaApplication;
        private AddinToolbar _toolbar;
        private CatiaService _catiaService;
        private ArasService _arasService;
        private ConfigManager _configManager;

        /// <summary>
        /// Gets the CATIA application instance.
        /// </summary>
        public object CatiaApplication => _catiaApplication;

        /// <summary>
        /// Gets the CATIA service for document operations.
        /// </summary>
        public CatiaService CatiaService => _catiaService;

        /// <summary>
        /// Gets the Aras service for PLM operations.
        /// </summary>
        public ArasService ArasService => _arasService;

        /// <summary>
        /// Gets the configuration manager.
        /// </summary>
        public ConfigManager ConfigManager => _configManager;

        /// <summary>
        /// Called when the add-in is loaded by CATIA.
        /// </summary>
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {
                Logger.Info("ArasCatiaAddin loading...");

                _catiaApplication = application;

                // Initialize services
                _configManager = new ConfigManager();
                _catiaService = new CatiaService(_catiaApplication);
                _arasService = new ArasService();

                // Create toolbar
                _toolbar = new AddinToolbar(this);
                _toolbar.CreateToolbar();

                // Auto-login if configured
                if (_configManager.Config.AutoLogin && !string.IsNullOrEmpty(_configManager.Config.ArasUsername))
                {
                    TryAutoLogin();
                }

                Logger.Info("ArasCatiaAddin loaded successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load add-in: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Called when the add-in is unloaded by CATIA.
        /// </summary>
        public void OnDisconnection(ext_DisconnectMode removeMode, ref Array custom)
        {
            try
            {
                Logger.Info("ArasCatiaAddin unloading...");

                // Remove toolbar
                if (_toolbar != null)
                {
                    _toolbar.RemoveToolbar();
                    _toolbar = null;
                }

                // Disconnect from Aras
                if (_arasService != null)
                {
                    _arasService.Disconnect();
                    _arasService = null;
                }

                // Cleanup CATIA service
                if (_catiaService != null)
                {
                    _catiaService.Dispose();
                    _catiaService = null;
                }

                _catiaApplication = null;

                Logger.Info("ArasCatiaAddin unloaded.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during unload: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Called after add-in is fully connected.
        /// </summary>
        public void OnAddInsUpdate(ref Array custom)
        {
            // Not used
        }

        /// <summary>
        /// Called when CATIA starts up (if add-in loaded at startup).
        /// </summary>
        public void OnStartupComplete(ref Array custom)
        {
            Logger.Debug("CATIA startup complete.");
        }

        /// <summary>
        /// Called when CATIA begins to shut down.
        /// </summary>
        public void OnBeginShutdown(ref Array custom)
        {
            Logger.Debug("CATIA beginning shutdown.");
        }

        /// <summary>
        /// Update toolbar button states based on login status.
        /// </summary>
        public void UpdateToolbarState()
        {
            _toolbar?.UpdateButtonStates(_arasService?.IsConnected ?? false);
        }

        /// <summary>
        /// Attempt auto-login with saved credentials.
        /// </summary>
        private void TryAutoLogin()
        {
            try
            {
                var config = _configManager.Config;
                if (string.IsNullOrEmpty(config.ArasPassword))
                    return;

                string errorMessage;
                if (_arasService.Connect(config.ArasServerUrl, config.ArasDatabase,
                    config.ArasUsername, config.ArasPassword, out errorMessage))
                {
                    Logger.Info($"Auto-login successful for user: {config.ArasUsername}");
                    UpdateToolbarState();
                }
                else
                {
                    Logger.Warning($"Auto-login failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Auto-login error: {ex.Message}");
            }
        }

        #region COM Registration

        /// <summary>
        /// Register the add-in in Windows registry for CATIA to find it.
        /// </summary>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            try
            {
                // Register in CATIA's add-in registry location
                string keyPath = @"SOFTWARE\CATIA\AddIns\" + type.GUID.ToString("B");

                using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("", "ArasCatiaAddin");
                        key.SetValue("CLSID", type.GUID.ToString("B"));
                        key.SetValue("Description", "Aras Innovator PLM Integration");
                        key.SetValue("LoadBehavior", 3); // Load at startup
                    }
                }

                // Also register in HKCU for non-admin installs
                string userKeyPath = @"SOFTWARE\CATIA\AddIns\" + type.GUID.ToString("B");
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(userKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("", "ArasCatiaAddin");
                        key.SetValue("CLSID", type.GUID.ToString("B"));
                        key.SetValue("Description", "Aras Innovator PLM Integration");
                        key.SetValue("LoadBehavior", 3);
                    }
                }

                Logger.Info("COM registration successful.");
            }
            catch (Exception ex)
            {
                Logger.Error($"COM registration failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Unregister the add-in from Windows registry.
        /// </summary>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            try
            {
                string keyPath = @"SOFTWARE\CATIA\AddIns\" + type.GUID.ToString("B");

                try { Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(keyPath); } catch { }
                try { Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(keyPath); } catch { }

                Logger.Info("COM unregistration successful.");
            }
            catch (Exception ex)
            {
                Logger.Error($"COM unregistration failed: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
