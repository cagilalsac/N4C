using AutoMapper;

namespace N4C.Models
{
    public class Config : Profile
    {
        public string Culture { get; private set; } = Settings.Culture;
        public string TitleTR { get; private set; } = "Kayıt";
        public string TitleEN { get; private set; } = "Record";

        public void SetCulture(string culture) => Culture = culture;

        public void SetTitle(string titleTR, string titleEN = default)
        {
            TitleTR = titleTR;
            TitleEN = titleEN;
        }

        public string Title => Culture == Cultures.TR ? TitleTR : TitleEN ?? "Record";

        public string NotFound => Culture == Cultures.TR ? $"{Title} bulunamadı" : $"{Title} not found";

        public string Found => Culture == Cultures.TR ? Title == "Kayıt" ? $"{Title.ToLower()} bulundu" : $"{Title.ToLower()} kaydı bulundu" :
                Title == "Record" ? $"{Title.ToLower()}(s) found" : $"{Title.ToLower()} record(s) found";

        public string Created => Culture == Cultures.TR ? $"{Title} başarıyla oluşturuldu" : $"{Title} created successfully";

        public string Updated => Culture == Cultures.TR ? $"{Title} başarıyla güncellendi" : $"{Title} updated successfully";

        public string Deleted => Culture == Cultures.TR ? $"{Title} başarıyla silindi" : $"{Title} deleted successfully";

        public string Success => Culture == Cultures.TR ? "İşlem başarıyla gerçekleştirildi." : "Operation successful.";

        public string Error => Culture == Cultures.TR ? "İşlem gerçekleştirilemedi!" : "Operation failed!";

        public string Unauthorized => Culture == Cultures.TR ? "Yetkisiz işlem!" : "Unauthorized operation!";

        public string Exception => Culture == Cultures.TR ? "Hata meydana geldi!" : "Exception occurred!";

        public string RelationsFound => Culture == Cultures.TR ? "İlişkili kayıtlar bulunmaktadır" : "Relational records found";

        public string True => "<i class='bx bx-check fs-3'></i>";

        public string False => "<i class='bx bx-x fs-3'></i>";
    }
}
