using N4C.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace N4C.Domain.Users
{
    public class User : Entity, IDeleted, IModified
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        public string Password { get; set; }

        public bool Active { get; set; } = true;

        public List<UserRole> UserRoles { get; private set; } = new List<UserRole>();

        [NotMapped]
        [Required]
        public List<int> RoleIds
        {
            get => UserRoles?.Select(ur => ur.RoleId).ToList();
            set => UserRoles = value?.Select(v => new UserRole() { RoleId = v }).ToList();
        }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
