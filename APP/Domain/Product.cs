using N4C.Attributes;
using N4C.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace APP.Domain
{
    public class Product : Entity, IModified, IFile
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        public decimal? UnitPrice { get; set; }

        public int? StockAmount { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Required]
        public int? CategoryId { get; set; }

        public Category _Category { get; set; }

        public List<ProductStore> _ProductStores { get; private set; } = new List<ProductStore>();

        [NotMapped]
        public List<int> StoreIds
        {
            get => _ProductStores?.Select(ps => ps.StoreId).ToList();
            set => _ProductStores = value?.Select(v => new ProductStore() { StoreId = v }).ToList();
        }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public string MainFile { get; set; }
        public List<string> OtherFiles { get; set; }
    }
}
