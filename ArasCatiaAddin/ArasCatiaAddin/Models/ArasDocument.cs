using System;

namespace ArasCatiaAddin.Models
{
    /// <summary>
    /// Represents a Document item from Aras.
    /// </summary>
    public class ArasDocument
    {
        public string Id { get; set; }
        public string ItemNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string LockedById { get; set; }
        public string LockedByName { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string DocumentType { get; set; }
    }
}
