﻿using N4C.Attributes;

namespace N4C.App.Services.Auth.Models
{
    public class RegisterRequest : Request
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [DisplayName("Şifre")]
        public string Password { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [DisplayName("Şifre Onay")]
        public string ConfirmPassword { get; set; }
    }
}
