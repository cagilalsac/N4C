using N4C.Attributes;
using N4C.Models;

namespace N4C.Users.App.Models
{
    public class N4CUserResponse : Response
    {
        [ExcelIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        [ExcelIgnore]
        public override string Guid { get => base.Guid; set => base.Guid = value; }

        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [DisplayName("Şifre")]
        [ExcelIgnore]
        public string Password { get; set; }

        [DisplayName("E-Posta", "E-Mail")]
        public string Email { get; set; }

        [DisplayName("Adı")]
        [ExcelIgnore]
        public string FirstName { get; set; }

        [DisplayName("Soyadı")]
        [ExcelIgnore]
        public string LastName { get; set; }

        [DisplayName("Tam Adı")]
        public string FullName { get; set; }

        [DisplayName("Roller")]
        [ExcelIgnore]
        public List<string> Roles { get; set; }

        [DisplayName("Roller", "Roles")]
        [ExcelIgnore]
        public List<int> RoleIds { get; set; }

        [DisplayName("Durum")]
        [ExcelIgnore]
        public N4CStatusResponse Status { get; set; }

        [DisplayName("Durum", "Status")]
        [ExcelIgnore]
        public bool Active { get; set; }

        [DisplayName("Durum", "Status")]
        [ExcelIgnore]
        public string ActiveS { get; set; }

        [DisplayName("Roller")]
        public string RolesE { get; set; }
    }
}
