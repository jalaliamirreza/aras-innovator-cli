namespace ArasCatiaAddin.Models
{
    /// <summary>
    /// Represents information extracted from a CATIA document.
    /// </summary>
    public class CatiaDocumentInfo
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string DocumentType { get; set; }
        public string PartNumber { get; set; }
        public string Nomenclature { get; set; }
        public string Revision { get; set; }
        public string Definition { get; set; }
        public bool IsSaved { get; set; }
    }
}
