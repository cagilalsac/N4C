using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using N4C.Domain;

namespace N4C.Users.App.Domain
{
    public class N4CUser : FileEntity
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        public string Password { get; set; }

        [StringLength(200, MinimumLength = 5)]
        public string Email { get; set; }

        [StringLength(75)]
        public string FirstName { get; set; }

        [StringLength(75)]
        public string LastName { get; set; }

        public List<N4CUserRole> UserRoles { get; private set; } = new();

        [NotMapped]
        [Required]
        public List<int> RoleIds
        {
            get => UserRoles?.Select(ur => ur.RoleId).ToList();
            set => UserRoles = value?.Select(v => new N4CUserRole() { RoleId = v }).ToList();
        }

        public int StatusId { get; set; }
        public N4CStatus Status { get; set; }
    }
}
