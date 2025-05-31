using N4C.Attributes;
using N4C.Models;

namespace N4C.Users.App.Models
{
    public class N4CRoleRequest : Request
    {
        [Required]
        [StringLength(50)]
        [DisplayName("Role Adı", "Role Name")]
        public string Name { get; set; }
    }
}
