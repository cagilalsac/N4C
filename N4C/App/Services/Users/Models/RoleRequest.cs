using N4C.Attributes;

namespace N4C.App.Services.Users.Models
{
    public class RoleRequest : Request
    {
        [Required]
        [StringLength(50)]
        [DisplayName("Role Adı", "Role Name")]
        public string Name { get; set; }
    }
}
