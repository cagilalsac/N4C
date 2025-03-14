using N4C.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace N4C.Domain.Users
{
    public class Role : Entity, IDeleted, IModified
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public List<UserRole> UserRoles { get; private set; } = new List<UserRole>();

        [NotMapped]
        public List<int> UserIds
        {
            get => UserRoles?.Select(ur => ur.UserId).ToList();
            set => UserRoles = value?.Select(v => new UserRole() { UserId = v }).ToList();
        }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
