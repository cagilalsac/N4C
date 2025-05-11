namespace N4C.App.Services.Auth.Models
{
    public class AuthViewModel
    {
        public LoginRequest LoginRequest { get; set; }
        public RegisterRequest RegisterRequest { get; set; }
        public int AuthMvcAction { get; set; }
    }
}
