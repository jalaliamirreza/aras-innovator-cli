using System;
using System.IO;
using Newtonsoft.Json;

namespace ArasCLI.Services
{
    /// <summary>
    /// Manages application configuration including Aras connection settings and CATIA workspace settings.
    /// </summary>
    public class ConfigManager
    {
        private static readonly string ConfigFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ArasCLI");

        private static readonly string ConfigFile = Path.Combine(ConfigFolder, "config.json");

        public AppConfig Config { get; private set; }

        public ConfigManager()
        {
            Load();
        }

        /// <summary>
        /// Load configuration from file.
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    string json = File.ReadAllText(ConfigFile);
                    Config = JsonConvert.DeserializeObject<AppConfig>(json);
                }
                else
                {
                    Config = new AppConfig();
                }
            }
            catch
            {
                Config = new AppConfig();
            }
        }

        /// <summary>
        /// Save configuration to file.
        /// </summary>
        public void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigFolder))
                {
                    Directory.CreateDirectory(ConfigFolder);
                }

                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(ConfigFile, json);
            }
            catch
            {
                // Silently fail - non-critical operation
            }
        }

        /// <summary>
        /// Get the configuration folder path.
        /// </summary>
        public static string GetConfigFolder()
        {
            return ConfigFolder;
        }

        /// <summary>
        /// Get the log file path.
        /// </summary>
        public static string GetLogFilePath()
        {
            return Path.Combine(ConfigFolder, "aras_cli.log");
        }
    }

    /// <summary>
    /// Application configuration settings.
    /// </summary>
    public class AppConfig
    {
        // Aras Connection Settings
        public string ArasServerUrl { get; set; } = "http://localhost/InnovatorServer";
        public string ArasDatabase { get; set; } = "InnovatorSolutions";
        public string ArasUsername { get; set; } = "admin";
        public bool RememberCredentials { get; set; } = false;

        // CATIA Workspace Settings
        public string LocalWorkspace { get; set; } = @"C:\ArasWorkspace";
        public bool AutoOpenAfterCheckout { get; set; } = true;
        public bool AutoSaveBeforeCheckin { get; set; } = true;

        // Property Mappings (CATIA to Aras)
        public PropertyMapping[] PropertyMappings { get; set; } = new PropertyMapping[]
        {
            new PropertyMapping { CatiaProperty = "PartNumber", ArasProperty = "item_number" },
            new PropertyMapping { CatiaProperty = "Nomenclature", ArasProperty = "name" },
            new PropertyMapping { CatiaProperty = "Revision", ArasProperty = "major_rev" },
            new PropertyMapping { CatiaProperty = "Definition", ArasProperty = "description" }
        };

        // File Type Mappings
        public FileTypeMapping[] FileTypeMappings { get; set; } = new FileTypeMapping[]
        {
            new FileTypeMapping { Extension = ".CATPart", ArasDocType = "3D Model", Description = "CATIA Part" },
            new FileTypeMapping { Extension = ".CATProduct", ArasDocType = "3D Model", Description = "CATIA Assembly" },
            new FileTypeMapping { Extension = ".CATDrawing", ArasDocType = "Drawing", Description = "CATIA Drawing" },
            new FileTypeMapping { Extension = ".pdf", ArasDocType = "Drawing", Description = "PDF Export" },
            new FileTypeMapping { Extension = ".stp", ArasDocType = "Exchange", Description = "STEP File" },
            new FileTypeMapping { Extension = ".step", ArasDocType = "Exchange", Description = "STEP File" }
        };

        // Recent Files
        public string[] RecentFiles { get; set; } = new string[0];

        // Window Settings
        public int WindowWidth { get; set; } = 800;
        public int WindowHeight { get; set; } = 600;
    }

    /// <summary>
    /// Property mapping between CATIA and Aras.
    /// </summary>
    public class PropertyMapping
    {
        public string CatiaProperty { get; set; }
        public string ArasProperty { get; set; }
    }

    /// <summary>
    /// File type mapping for CATIA files to Aras document types.
    /// </summary>
    public class FileTypeMapping
    {
        public string Extension { get; set; }
        public string ArasDocType { get; set; }
        public string Description { get; set; }
    }
}
