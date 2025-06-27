using AutoMapper;
using N4C.Extensions;

namespace N4C.Models
{
    public class ServiceConfig : Profile
    {
        public string Culture { get; private set; }
        public string TitleTR { get; private set; } = "Kayıt";
        public string TitleEN { get; private set; } = "Record";
        public bool ModelStateErrors { get; private set; } = true;
        public string FilesFolder { get; private set; } = "files";
        public double MaximumFileSizeInMb { get; private set; } = 5;
        public byte MaximumOtherFilesCount { get; private set; } = 25;
        public bool ExcelLicenseCommercial { get; private set; }
        public List<string> FileExtensions { get; private set; } = [".jpg", ".jpeg", ".png"];

        public Dictionary<string, string> FileMimeTypes { get; private set; } = new Dictionary<string, string>()
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".doc", "application/vnd.ms-word" },
            { ".docx", "application/vnd.ms-word" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".csv", "text/csv" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" }
        };

        public void SetCulture(string culture) => Culture = culture.HasNotAny(Settings.Culture);

        public void SetTitle(string titleTR, string titleEN = default)
        {
            TitleTR = titleTR.HasNotAny("Kayıt");
            TitleEN = titleEN.HasNotAny("Record");
        }

        public void SetModelStateErrors(bool modelStateErrors)
        {
            ModelStateErrors = modelStateErrors;
        }

        public void SetFilesFolder(string filesFolder) => FilesFolder = filesFolder;
        public void SetMaximumFileSizeInMb(double maximumFileSizeInMb) => MaximumFileSizeInMb = maximumFileSizeInMb;
        public void SetMaximumOtherFilesCount(byte maximumOtherFilesCount) => MaximumOtherFilesCount = maximumOtherFilesCount;
        public void SetExcelLicenseCommercial(bool excelLicenseCommercial) => ExcelLicenseCommercial = excelLicenseCommercial;

        public void SetFileExtensions(params string[] fileExtensions)
        {
            if (fileExtensions.Any())
            {
                FileExtensions.Clear();
                FileExtensions.AddRange(fileExtensions);
            }
        }

        public void SetFileMimeTypes(Dictionary<string, string> fileMimeTypes)
        {
            if (fileMimeTypes.Any())
            {
                fileMimeTypes.Clear();
                foreach (var fileMimeType in fileMimeTypes)
                {
                    FileMimeTypes.Add(fileMimeType.Key, fileMimeType.Value);
                }
            }
        }

        public string Title => Culture == Defaults.TR ? TitleTR : TitleEN.HasNotAny("Record");

        public string NotFound => Culture == Defaults.TR ? $"{Title} bulunamadı" : $"{Title} not found";

        public string Found => Culture == Defaults.TR ? Title == "Kayıt" ? $"{Title.ToLower()} bulundu" : $"{Title.ToLower()} kaydı bulundu" :
                Title == "Record" ? $"{Title.ToLower()}(s) found" : $"{Title.ToLower()} record(s) found";

        public string Created => Culture == Defaults.TR ? $"{Title} başarıyla oluşturuldu" : $"{Title} created successfully";

        public string Updated => Culture == Defaults.TR ? $"{Title} başarıyla güncellendi" : $"{Title} updated successfully";

        public string Deleted => Culture == Defaults.TR ? $"{Title} başarıyla silindi" : $"{Title} deleted successfully";

        public string Success => Culture == Defaults.TR ? "İşlem başarıyla gerçekleştirildi." : "Operation successful.";

        public string Error => Culture == Defaults.TR ? "İşlem gerçekleştirilemedi!" : "Operation failed!";

        public string Unauthorized => Culture == Defaults.TR ? "Yetkisiz işlem!" : "Unauthorized operation!";

        public string Exception => Culture == Defaults.TR ? "Hata meydana geldi!" : "Exception occurred!";

        public string RelationsFound => Culture == Defaults.TR ? "İlişkili kayıtlar bulunmaktadır" : "Relational records found";

        public string TrueHtml => "<i class='bx bx-check fs-3'></i>";

        public string FalseHtml => "<i class='bx bx-x fs-3'></i>";
    }
}
