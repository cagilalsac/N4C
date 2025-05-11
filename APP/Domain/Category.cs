using N4C.Attributes;
using N4C.Domain;

namespace APP.Domain
{
    public class Category : Entity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
