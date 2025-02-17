using N4C.Attributes;
using N4C.Domain;

namespace APP.Domain
{
    public class Store : Entity
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Name { get; set; }

        public bool Virtual { get; set; }

        public List<ProductStore> ProductStores { get; private set; } = new List<ProductStore>();
    }
}
