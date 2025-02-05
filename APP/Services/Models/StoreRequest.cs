using N4C.App;
using N4C.Attributes;

namespace APP.Services.Models
{
    public class StoreRequest : Request
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        [DisplayName("Adı")]
        public string Name { get; set; }

        [DisplayName("Sanal")]
        public bool Virtual { get; set; }
    }
}
