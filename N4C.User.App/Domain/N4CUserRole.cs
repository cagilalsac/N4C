using N4C.Domain;

namespace N4C.User.App.Domain
{
    public class N4CUserRole : Entity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public N4CUser User { get; set; }
        public N4CRole Role { get; set; }
    }
}
