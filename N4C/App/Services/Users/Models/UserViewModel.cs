namespace N4C.App.Services.Users.Models
{
    public class UserViewModel
    {
        public List<UserResponse> Data { get; set; }
        public UserRequest Request { get; set; }
        public int MvcAction { get; set; }
    }
}
