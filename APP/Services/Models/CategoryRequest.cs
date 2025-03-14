using N4C.App;
using N4C.Attributes;

namespace APP.Services.Models
{
    public class CategoryRequest : Request
    {
        [Required]
        [StringLength(100)]
        [DisplayName("Kategori Adı", "Category Name")]
        public string Name { get; set; }

        [DisplayName("Kategori Açıklaması", "Category Description")]
        [StringLength(500)]
        public string Description { get; set; }
    }
}
