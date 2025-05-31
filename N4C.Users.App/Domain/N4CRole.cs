using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using N4C.Domain;

namespace N4C.Users.App.Domain
{
    public class N4CRole : Entity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public List<N4CUserRole> UserRoles { get; private set; } = new();

        [NotMapped]
        public List<int> UserIds
        {
            get => UserRoles?.Select(ur => ur.UserId).ToList();
            set => UserRoles = value?.Select(v => new N4CUserRole() { UserId = v }).ToList();
        }
    }
}
