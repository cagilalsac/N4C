using Microsoft.AspNetCore.Http;
using N4C.App.Services.Files.Models;
using N4C.Extensions;
using OfficeOpenXml;
using System.Net;

namespace N4C.App.Services.Files
{
    public class FileService : Service
    {
        protected string FilesFolder { get; private set; }
        protected double FileMaximumSizeInMb { get; private set; }
        protected byte MaximumOtherFilesCount { get; private set; }
        protected List<string> FileExtensions { get; private set; }
        protected bool ExcelLicenseCommercial { get; private set; }

        protected Dictionary<string, string> FileMimeTypes { get; }

        protected HttpService HttpService { get; }

        public FileService(HttpService httpService, LogService logService) : base(logService)
        {
            HttpService = httpService;
            SetCulture(HttpService.Culture);
            FilesFolder = "files";
            FileMaximumSizeInMb = 5;
            MaximumOtherFilesCount = 25;
            FileExtensions = [".jpg", ".jpeg", ".png"];
            FileMimeTypes = new Dictionary<string, string>()
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
        }

        internal override void SetCulture(string culture)
        {
            base.SetCulture(culture);
            NotFound = Culture == Cultures.TR ? "Dosya bulunamadı!" : "File not found!";
            Found = Culture == Cultures.TR ? "dosya bulundu." : "file(s) found.";
            Created = Culture == Cultures.TR ? "Dosyalar başarıyla oluşturuldu" : "Files created successfully";
            Updated = Culture == Cultures.TR ? "Dosyalar başarıyla güncellendi" : "Files updated successfully";
            Deleted = Culture == Cultures.TR ? "Dosyalar başarıyla silindi" : "Files deleted successfully";
        }

        protected void SetFilesFolder(string folder)
        {
            FilesFolder = folder;
        }

        protected void SetFileMaximumSizeInMb(double maximumSizeInMb)
        {
            FileMaximumSizeInMb = maximumSizeInMb;
        }

        protected void SetMaximumOtherFilesCount(byte maximumOtherFilesCount)
        {
            MaximumOtherFilesCount = maximumOtherFilesCount;
        }

        protected void SetFileExtensions(params string[] extensions)
        {
            FileExtensions = extensions.ToList();
        }

        protected void SetExcelLicenseCommercial(bool excelLicenseCommercial)
        {
            ExcelLicenseCommercial = excelLicenseCommercial;
        }

        protected string GetFileExtension(IFormFile formFile)
        {
            return Path.GetExtension(formFile.FileName).ToLower();
        }

        protected string GetFileExtension(string filePath)
        {
            return $".{filePath.Split('.')[1]}";
        }

        protected virtual string GetFilePath(IFormFile formFile)
        {
            return Path.Combine("wwwroot", GetFileFolder(), $"{Guid.NewGuid().ToString()}{GetFileExtension(formFile)}");
        }

        protected virtual string GetFilePath(string filePath, bool wwwroot = false)
        {
            return string.IsNullOrWhiteSpace(filePath) ? null : wwwroot ? $"wwwroot{filePath}" : filePath.Substring(7).Replace(@"\", "/");
        }

        protected string GetFileContentType(string filePath, bool data = false, bool base64 = false)
        {
            var fileExtension = GetFileExtension(filePath);
            var contentType = FileMimeTypes[fileExtension];
            if (data)
                contentType = "data:" + contentType;
            if (base64)
                contentType = contentType + ";base64,";
            return contentType;
        }

        public string GetFileFolder(string filePath = null)
        {
            return string.IsNullOrWhiteSpace(filePath) ? FilesFolder : filePath.Split('/')[1];
        }

        public string GetFileName(string filePath, bool extension = true)
        {
            var fileName = filePath.Split('/')[filePath.Split('/').Length - 1].Split('.')[0];
            if (extension)
                fileName += GetFileExtension(filePath);
            return fileName;
        }

        public int GetFileOrder(string filePath)
        {
            return Convert.ToInt32(filePath.Split('/')[2]);
        }

        public List<string> GetOtherFiles(List<FileResponse> otherFilePaths, int orderInitialValue, int orderPaddingTotalWidth = 3)
        {
            List<string> otherFiles = null;
            if (otherFilePaths is not null && otherFilePaths.Any())
            {
                otherFiles = new List<string>();
                string orderValue;
                for (int i = 0; i < otherFilePaths.Count; i++)
                {
                    orderValue = orderInitialValue++.ToString().PadLeft(orderPaddingTotalWidth, '0');
                    otherFiles.Add($"/{GetFileFolder(otherFilePaths[i].MainFile)}/{orderValue}/{GetFileName(otherFilePaths[i].MainFile)}");
                }
            }
            return otherFiles;
        }

        public void GetOtherFiles(List<string> filePaths)
        {
            if (filePaths is not null && filePaths.Any())
            {
                for (int i = 0; i < filePaths.Count; i++)
                {
                    filePaths[i] = $"/{GetFileFolder(filePaths[i])}/{GetFileName(filePaths[i])}";
                }
            }
        }

        public virtual Result ValidateOtherFiles(List<IFormFile> formFiles, List<string> currentFilePaths = null)
        {
            var otherFilesCount = 0;
            if (formFiles is not null)
                otherFilesCount += formFiles.Count;
            if (currentFilePaths is not null)
                otherFilesCount += currentFilePaths.Count;
            if (otherFilesCount > MaximumOtherFilesCount)
                return Error(Culture == Cultures.TR ? $"Diğer dosya sayısı maksimum {MaximumOtherFilesCount} olmalıdır!" :
                    $"Other files count must be maximum {MaximumOtherFilesCount}!");
            return Success();
        }

        public virtual Result<FileResponse> CreateFile(IFormFile formFile)
        {
            FileResponse fileResponse = null;
            try
            {
                string filePath = null;
                if (formFile is not null && formFile.Length > 0)
                {
                    var result = ValidateFile(formFile);
                    if (result.Success)
                    {
                        filePath = GetFilePath(formFile);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            formFile.CopyTo(fileStream);
                        }
                        fileResponse = new FileResponse()
                        {
                            MainFile = GetFilePath(filePath)
                        };
                        return Success(fileResponse, Created);
                    }
                    return Error(fileResponse, result);
                }
                fileResponse = new FileResponse()
                {
                    MainFile = GetFilePath(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                LogService.LogError($"FileServiceException: {GetType().Name}.CreateFile(): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        public virtual Result<FileResponse> UpdateFile(IFormFile formFile, string currentFilePath)
        {
            FileResponse fileResponse = null;
            try
            {
                string filePath = GetFilePath(currentFilePath, true);
                if (formFile is not null && formFile.Length > 0)
                {
                    var result = ValidateFile(formFile);
                    if (result.Success)
                    {
                        result = DeleteFile(currentFilePath);
                        if (result.Success)
                        {
                            filePath = GetFilePath(formFile);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                formFile.CopyTo(fileStream);
                            }
                            fileResponse = new FileResponse()
                            {
                                MainFile = GetFilePath(filePath)
                            };
                            return Success(fileResponse, Updated);
                        }
                    }
                    return Error(fileResponse, result);
                }
                fileResponse = new FileResponse()
                {
                    MainFile = GetFilePath(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                LogService.LogError($"FileServiceException: {GetType().Name}.UpdateFile(Path = {currentFilePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        public virtual Result<FileResponse> DeleteFile(string currentFilePath)
        {
            FileResponse fileResponse = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(currentFilePath))
                {
                    currentFilePath = GetFilePath(currentFilePath, true);
                    if (File.Exists(currentFilePath))
                    {
                        File.Delete(currentFilePath);
                        fileResponse = new FileResponse()
                        {
                            MainFile = GetFilePath(currentFilePath)
                        };
                        return Success(fileResponse, Deleted);
                    }
                }
                fileResponse = new FileResponse()
                {
                    MainFile = GetFilePath(currentFilePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                LogService.LogError($"FileServiceException: {GetType().Name}.DeleteFile(Path = {currentFilePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        protected virtual Result ValidateFile(IFormFile formFile)
        {
            if (formFile.Length > FileMaximumSizeInMb * Math.Pow(1024, 2))
                return Error(Culture == Cultures.TR ? $"Geçersiz dosya boyutu, geçerli maksimum dosya boyutu: {FileMaximumSizeInMb} MB!" :
                    $"Invalid file size, valid maxiumum file size: {FileMaximumSizeInMb} MB!");
            else if (!FileExtensions.Contains(GetFileExtension(formFile)))
                return Error(Culture == Cultures.TR ? $"Geçersiz dosya uzantısı, geçerli dosya uzantıları: {string.Join(", ", FileExtensions)}!" :
                    $"Invalid file extension, valid file extensions: {string.Join(", ", FileExtensions)}!");
            return Success();
        }

        public virtual Result<List<FileResponse>> CreateFiles(List<IFormFile> formFiles)
        {
            List<FileResponse> fileResponseList = null;
            FileResponse fileResponse = null;
            Result<FileResponse> result;
            Result validationResult = Success();
            if (formFiles is not null && formFiles.Any())
            {
                fileResponseList = new List<FileResponse>();
                foreach (var formFile in formFiles)
                {
                    validationResult = ValidateFile(formFile);
                    if (!validationResult.Success)
                        break;
                }
                if (validationResult.Success)
                {
                    result = Success(fileResponse);
                    foreach (var formFile in formFiles)
                    {
                        result = CreateFile(formFile);
                        if (!result.Success)
                            break;
                        fileResponseList.Add(result.Data);
                    }
                    if (!result.Success)
                        return Error(fileResponseList, result);
                }
                else
                {
                    return Error(fileResponseList, validationResult);
                }
            }
            return Success(fileResponseList, Created);
        }

        public virtual Result<List<FileResponse>> DeleteFiles(List<string> currentFilePaths)
        {
            List<FileResponse> fileResponseList = null;
            FileResponse fileResponse = null;
            Result<FileResponse> result = Success(fileResponse);
            if (currentFilePaths is not null && currentFilePaths.Any())
            {
                fileResponseList = new List<FileResponse>();
                foreach (var currentFilePath in currentFilePaths)
                {
                    result = DeleteFile(currentFilePath);
                    if (!result.Success)
                        break;
                    fileResponseList.Add(result.Data);
                }
                if (!result.Success)
                    return Error(fileResponseList, result);
            }
            return Success(fileResponseList, Deleted);
        }

        public virtual void GetExcel<T>(List<T> list) where T : class, new()
        {
            try
            {
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace("-", "").Replace(":", "").Replace(" ", "_");
                var fileName = Culture == Cultures.TR ? $"Rapor_{dateTime}.xlsx" : $"Report_{dateTime}.xlsx";
                var worksheet = Culture == Cultures.TR ? "Sayfa1" : "Sheet1";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                ExcelPackage.LicenseContext = ExcelLicenseCommercial ? LicenseContext.Commercial : LicenseContext.NonCommercial;
                var excelPackage = new ExcelPackage();
                var excelWorksheet = excelPackage.Workbook.Worksheets.Add(worksheet);
                excelWorksheet.Cells["A1"].LoadFromDataTable(list.ConvertToDataTable(Culture), true);
                excelWorksheet.Cells["A1:AZ1"].Style.Font.Bold = true;
                excelWorksheet.Cells["A1:AZ1"].AutoFilter = true;
                excelWorksheet.Cells["A:AZ"].AutoFitColumns();
                HttpService.GetResponse(excelPackage.GetAsByteArray(), fileName, contentType);
            }
            catch (Exception exception)
            {
                LogService.LogError($"FileServiceException: {GetType().Name}.GetExcel(): {exception.Message}");
            }
        }

        public virtual Result<FileResponse> GetFile(string filePath, bool useOctetStreamContentType = false)
        {
            FileResponse fileResponse = null;
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return Error(fileResponse, HttpStatusCode.NotFound);
                fileResponse = new FileResponse()
                {
                    FileStream = new FileStream(GetFilePath(filePath, true), FileMode.Open),
                    FileContentType = useOctetStreamContentType ? "application/octet-stream" : GetFileContentType(filePath),
                    FileName = GetFileName(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                LogService.LogError($"FileServiceException: {GetType().Name}.GetFile(Path = {filePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }
    }
}
