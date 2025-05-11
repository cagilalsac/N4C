using N4C.App;
using N4C.Attributes;

namespace APP.Services.Models
{
    public class CategoryResponse : Response
    {
        [DisplayName("Kategori Adı", "Category Name")]
        public string Name { get; set; }

        [DisplayName("Kategori Açıklaması", "Category Description")]
        public string Description { get; set; }
    }
}
