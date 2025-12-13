namespace ArasCatiaAddin.Models
{
    /// <summary>
    /// Represents user settings for the add-in.
    /// </summary>
    public class AppConfig
    {
        // Connection settings
        public string ArasServerUrl { get; set; }
        public string ArasDatabase { get; set; }
        public string ArasUsername { get; set; }
        public string ArasPassword { get; set; }
        public bool RememberPassword { get; set; }
        public bool AutoLogin { get; set; }

        // Workspace settings
        public string LocalWorkspace { get; set; }
        public bool OverwriteExisting { get; set; }

        // Check-in settings
        public bool AutoGenerateItemNumber { get; set; }
        public string ItemNumberPrefix { get; set; }
        public string DefaultDocumentType { get; set; }
        public bool ConfirmBeforeCheckin { get; set; }

        // BOM sync settings
        public bool CreateMissingParts { get; set; }
        public bool SyncProperties { get; set; }
    }
}
