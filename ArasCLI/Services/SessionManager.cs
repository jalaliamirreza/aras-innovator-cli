using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Aras.IOM;

namespace ArasCLI.Services
{
    /// <summary>
    /// Manages Aras Innovator session across multiple process instances.
    /// Uses encrypted file-based session storage for security.
    /// </summary>
    public class SessionManager
    {
        private static readonly string SessionFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ArasCLI");

        private static readonly string SessionFile = Path.Combine(SessionFolder, "session.dat");
        private static readonly string LockFile = Path.Combine(SessionFolder, "session.lock");

        // Session timeout in hours
        private const int SessionTimeoutHours = 8;

        // Encryption key derived from machine-specific data
        private static readonly byte[] EncryptionKey = GetMachineKey();

        // Current session state
        private static HttpServerConnection _connection;
        private static Innovator _innovator;
        private static SessionData _sessionData;

        /// <summary>
        /// Check if there is an active session.
        /// </summary>
        public static bool HasActiveSession
        {
            get
            {
                // First check in-memory session
                if (_connection != null && _innovator != null)
                {
                    if (ValidateConnection())
                        return true;

                    // Connection is dead, clear it
                    ClearInMemorySession();
                }

                // Try to restore from file
                return TryRestoreSession();
            }
        }

        /// <summary>
        /// Get the current Innovator instance.
        /// </summary>
        public static Innovator Innovator => _innovator;

        /// <summary>
        /// Get the current connection.
        /// </summary>
        public static HttpServerConnection Connection => _connection;

        /// <summary>
        /// Get the logged in username.
        /// </summary>
        public static string Username => _sessionData?.Username;

        /// <summary>
        /// Get the session start time.
        /// </summary>
        public static DateTime? LoginTime => _sessionData?.LoginTime;

        /// <summary>
        /// Login to Aras and create a new session.
        /// </summary>
        public static LoginResult Login(string serverUrl, string database, string username, string password)
        {
            try
            {
                // Create connection
                var connection = IomFactory.CreateHttpServerConnection(
                    serverUrl, database, username, password);

                // Attempt login
                Item loginResult = connection.Login();

                if (loginResult.isError())
                {
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = loginResult.getErrorString()
                    };
                }

                // Store session
                _connection = connection;
                _innovator = new Innovator(connection);
                _sessionData = new SessionData
                {
                    ServerUrl = serverUrl,
                    Database = database,
                    Username = username,
                    Password = password, // Stored encrypted
                    LoginTime = DateTime.Now
                };

                // Save session to file
                SaveSession();

                return new LoginResult
                {
                    Success = true,
                    Username = username
                };
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Logout and clear the session.
        /// </summary>
        public static void Logout()
        {
            try
            {
                if (_connection != null)
                {
                    _connection.Logout();
                }
            }
            catch { }

            ClearInMemorySession();
            DeleteSessionFile();
        }

        /// <summary>
        /// Try to restore session from file.
        /// </summary>
        private static bool TryRestoreSession()
        {
            try
            {
                if (!File.Exists(SessionFile))
                    return false;

                // Read and decrypt session data
                byte[] encryptedData = File.ReadAllBytes(SessionFile);
                string json = Decrypt(encryptedData);
                _sessionData = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionData>(json);

                // Check if session is expired
                if ((DateTime.Now - _sessionData.LoginTime).TotalHours > SessionTimeoutHours)
                {
                    DeleteSessionFile();
                    _sessionData = null;
                    return false;
                }

                // Try to reconnect
                _connection = IomFactory.CreateHttpServerConnection(
                    _sessionData.ServerUrl,
                    _sessionData.Database,
                    _sessionData.Username,
                    _sessionData.Password);

                Item loginResult = _connection.Login();

                if (loginResult.isError())
                {
                    ClearInMemorySession();
                    DeleteSessionFile();
                    return false;
                }

                _innovator = new Innovator(_connection);
                _sessionData.LoginTime = DateTime.Now; // Refresh login time
                SaveSession();

                return true;
            }
            catch
            {
                ClearInMemorySession();
                DeleteSessionFile();
                return false;
            }
        }

        /// <summary>
        /// Validate the current connection is still alive.
        /// </summary>
        private static bool ValidateConnection()
        {
            try
            {
                Item test = _innovator.newItem("User", "get");
                test.setAttribute("select", "id");
                test.setAttribute("maxRecords", "1");
                Item result = test.apply();
                return !result.isError();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save session to encrypted file.
        /// </summary>
        private static void SaveSession()
        {
            try
            {
                if (!Directory.Exists(SessionFolder))
                    Directory.CreateDirectory(SessionFolder);

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_sessionData);
                byte[] encryptedData = Encrypt(json);
                File.WriteAllBytes(SessionFile, encryptedData);
            }
            catch { }
        }

        /// <summary>
        /// Delete the session file.
        /// </summary>
        private static void DeleteSessionFile()
        {
            try
            {
                if (File.Exists(SessionFile))
                    File.Delete(SessionFile);
            }
            catch { }
        }

        /// <summary>
        /// Clear in-memory session.
        /// </summary>
        private static void ClearInMemorySession()
        {
            _connection = null;
            _innovator = null;
            _sessionData = null;
        }

        /// <summary>
        /// Get machine-specific encryption key.
        /// </summary>
        private static byte[] GetMachineKey()
        {
            // Use machine name + user name as key source
            string keySource = Environment.MachineName + Environment.UserName + "ArasCLI_Session_Key";
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource));
            }
        }

        /// <summary>
        /// Encrypt string data.
        /// </summary>
        private static byte[] Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    // Write IV first
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypt byte data.
        /// </summary>
        private static string Decrypt(byte[] cipherData)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;

                // Read IV from beginning of data
                byte[] iv = new byte[16];
                Array.Copy(cipherData, 0, iv, 0, 16);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(cipherData, 16, cipherData.Length - 16))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Get session info for display.
        /// </summary>
        public static SessionInfo GetSessionInfo()
        {
            if (_sessionData == null)
            {
                // Try to load from file without connecting
                try
                {
                    if (File.Exists(SessionFile))
                    {
                        byte[] encryptedData = File.ReadAllBytes(SessionFile);
                        string json = Decrypt(encryptedData);
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionData>(json);

                        return new SessionInfo
                        {
                            IsLoggedIn = false,
                            ServerUrl = data.ServerUrl,
                            Database = data.Database,
                            Username = data.Username,
                            HasSavedSession = true,
                            SessionExpired = (DateTime.Now - data.LoginTime).TotalHours > SessionTimeoutHours
                        };
                    }
                }
                catch { }

                return new SessionInfo { IsLoggedIn = false, HasSavedSession = false };
            }

            return new SessionInfo
            {
                IsLoggedIn = _connection != null,
                ServerUrl = _sessionData.ServerUrl,
                Database = _sessionData.Database,
                Username = _sessionData.Username,
                LoginTime = _sessionData.LoginTime,
                HasSavedSession = true,
                SessionExpired = false
            };
        }
    }

    /// <summary>
    /// Session data stored in encrypted file.
    /// </summary>
    internal class SessionData
    {
        public string ServerUrl { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime LoginTime { get; set; }
    }

    /// <summary>
    /// Login result.
    /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Username { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Session info for display.
    /// </summary>
    public class SessionInfo
    {
        public bool IsLoggedIn { get; set; }
        public string ServerUrl { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public DateTime? LoginTime { get; set; }
        public bool HasSavedSession { get; set; }
        public bool SessionExpired { get; set; }

        public string SessionDuration
        {
            get
            {
                if (!LoginTime.HasValue) return "";
                var duration = DateTime.Now - LoginTime.Value;
                return duration.TotalMinutes < 60
                    ? $"{(int)duration.TotalMinutes} min"
                    : $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }
        }
    }
}
