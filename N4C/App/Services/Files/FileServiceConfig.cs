namespace N4C.App.Services.Files
{
    public class FileServiceConfig : IServiceConfig
    {
        public string Culture { get; set; } = Settings.Culture;
        public string TitleTR { get; set; } = "Dosya";
        public string TitleEN { get; set; } = "File";
        public string FilesFolder { get; set; } = "files";
        public double MaximumFileSizeInMb { get; set; } = 5;
        public byte MaximumOtherFilesCount { get; set; } = 25;
        public bool ExcelLicenseCommercial { get; set; }
        public Dictionary<string, string> FileMimeTypes { get; set; } = new Dictionary<string, string>()
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
        public List<string> FileExtensions { get; private set; } = [".jpg", ".jpeg", ".png"];

        public void SetFileExtensions(params string[] fileExtensions)
        {
            if (fileExtensions.Any())
            {
                FileExtensions.Clear();
                FileExtensions.AddRange(fileExtensions);
            }
        }
    }
}
