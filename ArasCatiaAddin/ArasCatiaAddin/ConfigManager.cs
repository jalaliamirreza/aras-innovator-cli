using System;
using System.IO;
using ArasCatiaAddin.Models;
using Newtonsoft.Json;

namespace ArasCatiaAddin
{
    /// <summary>
    /// Manages application configuration loading and saving.
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configDirectory;
        private readonly string _configFilePath;
        private AppConfig _config;

        public ConfigManager()
        {
            _configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ArasCatiaAddin");

            _configFilePath = Path.Combine(_configDirectory, "config.json");

            LoadConfig();
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public AppConfig Config => _config;

        /// <summary>
        /// Load configuration from file.
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    _config = JsonConvert.DeserializeObject<AppConfig>(json);

                    // Decrypt password if saved
                    if (!string.IsNullOrEmpty(_config.ArasPassword) && _config.RememberPassword)
                    {
                        _config.ArasPassword = DecryptPassword(_config.ArasPassword);
                    }
                }
                else
                {
                    _config = CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load config: {ex.Message}", ex);
                _config = CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Save configuration to file.
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                if (!Directory.Exists(_configDirectory))
                {
                    Directory.CreateDirectory(_configDirectory);
                }

                // Create a copy for saving (to encrypt password)
                var configToSave = new AppConfig
                {
                    ArasServerUrl = _config.ArasServerUrl,
                    ArasDatabase = _config.ArasDatabase,
                    ArasUsername = _config.ArasUsername,
                    RememberPassword = _config.RememberPassword,
                    AutoLogin = _config.AutoLogin,
                    LocalWorkspace = _config.LocalWorkspace,
                    OverwriteExisting = _config.OverwriteExisting,
                    AutoGenerateItemNumber = _config.AutoGenerateItemNumber,
                    ItemNumberPrefix = _config.ItemNumberPrefix,
                    DefaultDocumentType = _config.DefaultDocumentType,
                    ConfirmBeforeCheckin = _config.ConfirmBeforeCheckin,
                    CreateMissingParts = _config.CreateMissingParts,
                    SyncProperties = _config.SyncProperties
                };

                // Encrypt password if saving
                if (_config.RememberPassword && !string.IsNullOrEmpty(_config.ArasPassword))
                {
                    configToSave.ArasPassword = EncryptPassword(_config.ArasPassword);
                }
                else
                {
                    configToSave.ArasPassword = null;
                }

                string json = JsonConvert.SerializeObject(configToSave, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);

                Logger.Debug("Configuration saved.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save config: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Reset configuration to defaults.
        /// </summary>
        public void ResetConfig()
        {
            _config = CreateDefaultConfig();
            SaveConfig();
        }

        /// <summary>
        /// Create default configuration.
        /// </summary>
        private AppConfig CreateDefaultConfig()
        {
            return new AppConfig
            {
                ArasServerUrl = "http://localhost/InnovatorServer",
                ArasDatabase = "InnovatorSolutions",
                ArasUsername = "",
                ArasPassword = null,
                RememberPassword = false,
                AutoLogin = false,
                LocalWorkspace = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ArasWorkspace"),
                OverwriteExisting = false,
                AutoGenerateItemNumber = true,
                ItemNumberPrefix = "DOC",
                DefaultDocumentType = "3D Model",
                ConfirmBeforeCheckin = true,
                CreateMissingParts = true,
                SyncProperties = true
            };
        }

        /// <summary>
        /// Simple password encryption using DPAPI.
        /// </summary>
        private string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return null;

            try
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] encrypted = System.Security.Cryptography.ProtectedData.Protect(
                    data,
                    null,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch
            {
                return password; // Fallback to plaintext if DPAPI fails
            }
        }

        /// <summary>
        /// Simple password decryption using DPAPI.
        /// </summary>
        private string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword)) return null;

            try
            {
                byte[] data = Convert.FromBase64String(encryptedPassword);
                byte[] decrypted = System.Security.Cryptography.ProtectedData.Unprotect(
                    data,
                    null,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return System.Text.Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return encryptedPassword; // Assume plaintext if decryption fails
            }
        }

        /// <summary>
        /// Ensure workspace directory exists.
        /// </summary>
        public void EnsureWorkspaceExists()
        {
            if (!Directory.Exists(_config.LocalWorkspace))
            {
                try
                {
                    Directory.CreateDirectory(_config.LocalWorkspace);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to create workspace: {ex.Message}", ex);
                }
            }
        }
    }
}
