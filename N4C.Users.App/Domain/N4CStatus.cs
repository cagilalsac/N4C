using N4C.Domain;
using System.ComponentModel.DataAnnotations;

namespace N4C.Users.App.Domain
{
    public class N4CStatus : Entity
    {
        [Required, StringLength(50)]
        public string Title { get; set; }

        public List<N4CUser> Users { get; set; } = new();
    }
}
