using System.Collections.Generic;

namespace ArasCatiaAddin.Models
{
    /// <summary>
    /// Sync status for BOM items.
    /// </summary>
    public enum SyncStatus
    {
        New,        // Not in Aras, will be created
        Exists,     // Already in Aras, matches
        Modified,   // In Aras but different
        Error       // Problem detected
    }

    /// <summary>
    /// Represents an item in a bill of materials.
    /// </summary>
    public class BomItem
    {
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public string Nomenclature { get; set; }
        public string Revision { get; set; }
        public decimal Quantity { get; set; }
        public int Level { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public List<BomItem> Children { get; set; }
        public string ArasPartId { get; set; }
        public SyncStatus SyncStatus { get; set; }

        public BomItem()
        {
            Children = new List<BomItem>();
            Quantity = 1;
            SyncStatus = SyncStatus.New;
        }
    }
}
