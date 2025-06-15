using N4C.Attributes;
using N4C.Models;
using System.Text.Json.Serialization;

namespace N4C.Users.App.Models
{
    public class N4CUserResponse : FileResponse
    {
        [ExcelIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        [ExcelIgnore]
        public override string Guid { get => base.Guid; set => base.Guid = value; }

        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [DisplayName("Şifre")]
        [ExcelIgnore]
        [JsonIgnore]
        public string Password { get; set; }

        [DisplayName("E-Posta", "E-Mail")]
        public string Email { get; set; }

        [ExcelIgnore]
        public string FirstName { get; set; }

        [ExcelIgnore]
        public string LastName { get; set; }

        [DisplayName("Tam Adı")]
        [JsonIgnore]
        public string FullName { get; set; }

        [ExcelIgnore]
        public List<int> RoleIds { get; set; }

        [DisplayName("Roller")]
        [ExcelIgnore]
        public List<string> Roles { get; set; }

        [DisplayName("Roller")]
        [JsonIgnore]
        public string Roles_ { get; set; }

        [DisplayName("Durum", "Status")]
        [ExcelIgnore]
        [JsonIgnore]
        public int StatusId { get; set; }

        [ExcelIgnore]
        public N4CStatusResponse Status { get; set; }

        [ExcelIgnore]
        [JsonIgnore]
        public bool Active { get; set; }

        [DisplayName("Durum", "Status")]
        [JsonIgnore]
        public string Active_ { get; set; }

        [DisplayName("Durum", "Status")]
        [ExcelIgnore]
        [JsonIgnore]
        public string Active_Html { get; set; }
    }
}
