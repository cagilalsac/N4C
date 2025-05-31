using N4C.Attributes;
using N4C.Models;

namespace N4C.Users.App.Models
{
    public class N4CStatusResponse : Response
    {
        [DisplayName("Durum", "Status")]
        public string Title { get; set; }

        [DisplayName("Kullanıcılar")]
        public string Users { get; set; }
    }
}
