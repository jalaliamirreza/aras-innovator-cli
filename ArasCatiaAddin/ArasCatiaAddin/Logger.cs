using System;
using System.IO;

namespace ArasCatiaAddin
{
    /// <summary>
    /// Simple file-based logger for the add-in.
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDirectory;
        private static string _logFilePath;
        private static bool _initialized;

        /// <summary>
        /// Initialize the logger.
        /// </summary>
        private static void Initialize()
        {
            if (_initialized) return;

            try
            {
                _logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ArasCatiaAddin",
                    "Logs");

                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }

                string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                _logFilePath = Path.Combine(_logDirectory, $"log_{dateStr}.txt");

                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }

        /// <summary>
        /// Log a debug message.
        /// </summary>
        public static void Debug(string message)
        {
            Log("DEBUG", message);
        }

        /// <summary>
        /// Log an info message.
        /// </summary>
        public static void Info(string message)
        {
            Log("INFO", message);
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        public static void Warning(string message)
        {
            Log("WARNING", message);
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            if (ex != null)
            {
                message += $"\nException: {ex.GetType().Name}: {ex.Message}\nStack: {ex.StackTrace}";
            }
            Log("ERROR", message);
        }

        /// <summary>
        /// Write a log entry.
        /// </summary>
        private static void Log(string level, string message)
        {
            if (!_initialized) Initialize();
            if (!_initialized || string.IsNullOrEmpty(_logFilePath)) return;

            lock (_lock)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logEntry = $"[{timestamp}] [{level}] {message}";

                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);

                    // Also write to debug output for Visual Studio debugging
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
                catch
                {
                    // Silently fail - logging should not crash the app
                }
            }
        }

        /// <summary>
        /// Get the log directory path.
        /// </summary>
        public static string LogDirectory
        {
            get
            {
                if (!_initialized) Initialize();
                return _logDirectory;
            }
        }
    }
}
