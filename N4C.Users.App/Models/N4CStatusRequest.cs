using N4C.Attributes;
using N4C.Models;

namespace N4C.Users.App.Models
{
    public class N4CStatusRequest : Request
    {
        [Required, StringLength(50), DisplayName("Durum", "Status")]
        public string Title { get; set; }
    }
}
