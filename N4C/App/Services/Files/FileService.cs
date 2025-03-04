using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using N4C.App.Services.Files.Models;
using N4C.Extensions;
using OfficeOpenXml;
using System.Net;

namespace N4C.App.Services.Files
{
    public class FileService : Service
    {
        protected string Folder { get; private set; }
        protected double MaximumSizeInMb { get; private set; }
        protected byte MaximumOtherFilesCount { get; private set; }
        protected List<string> Extensions { get; private set; }
        protected bool ExcelLicenseCommercial { get; private set; }
        protected string FilesCreated { get; private set; }
        protected string FilesUpdated { get; private set; }
        protected string FilesDeleted { get; private set; }

        private readonly Dictionary<string, string> _mimeTypes;

        public FileService(HttpService httpService, ILogger<Service> logger) : base(httpService, logger)
        {
            NotFound = Culture == Cultures.TR ? "Dosya bulunamadı!" : "File not found!";
            Folder = "files";
            MaximumSizeInMb = 5;
            MaximumOtherFilesCount = 25;
            Extensions = [".jpg", ".jpeg", ".png"];
            FilesCreated = Culture == Cultures.TR ? "Dosyalar başarıyla oluşturuldu" : "Files created successfully";
            FilesUpdated = Culture == Cultures.TR ? "Dosyalar başarıyla güncellendi" : "Files updated successfully";
            FilesDeleted = Culture == Cultures.TR ? "Dosyalar başarıyla silindi" : "Files deleted successfully";
            _mimeTypes = new Dictionary<string, string>()
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

        protected void SetFolder(string folder)
        {
            Folder = folder;
        }

        protected void SetMaximumSizeInMb(double maximumSizeInMb)
        {
            MaximumSizeInMb = maximumSizeInMb;
        }

        protected void SetMaximumOtherFilesCount(byte maximumOtherFilesCount)
        {
            MaximumOtherFilesCount = maximumOtherFilesCount;
        }

        protected void SetExtensions(params string[] extensions)
        {
            Extensions = extensions.ToList();
        }

        protected void SetExcelLicenseCommercial(bool excelLicenseCommercial)
        {
            ExcelLicenseCommercial = excelLicenseCommercial;
        }

        private string GetFileExtension(IFormFile formFile)
        {
            return Path.GetExtension(formFile.FileName).ToLower();
        }

        private string GetFileExtension(string filePath)
        {
            return $".{filePath.Split('.')[1]}";
        }

        private string GetFilePath(IFormFile formFile)
        {
            return Path.Combine("wwwroot", GetFileFolder(), $"{Guid.NewGuid().ToString()}{GetFileExtension(formFile)}");
        }

        private string GetFilePath(string filePath, bool wwwroot = false)
        {
            return string.IsNullOrWhiteSpace(filePath) ? null : wwwroot ? $"wwwroot{filePath}" : filePath.Substring(7).Replace(@"\", "/");
        }

        private string GetContentType(string filePath, bool data = false, bool base64 = false)
        {
            var fileExtension = GetFileExtension(filePath);
            var contentType = _mimeTypes[fileExtension];
            if (data)
                contentType = "data:" + contentType;
            if (base64)
                contentType = contentType + ";base64,";
            return contentType;
        }

        public string GetFileFolder(string filePath = null)
        {
            return string.IsNullOrWhiteSpace(filePath) ? Folder : filePath.Split('/')[1];
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

        public Result ValidateOtherFiles(List<IFormFile> formFiles, List<string> currentFilePaths = null)
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

        public Result<FileResponse> Create(IFormFile formFile)
        {
            FileResponse fileResponse = null;
            try
            {
                string filePath = null;
                if (formFile is not null && formFile.Length > 0)
                {
                    var result = Validate(formFile);
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
                        return Success(fileResponse, FilesCreated);
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
                Logger.LogError($"FileServiceException: {GetType().Name}.Create(Path = {fileResponse?.MainFile}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        public Result<FileResponse> Update(IFormFile formFile, string currentFilePath)
        {
            FileResponse fileResponse = null;
            try
            {
                string filePath = GetFilePath(currentFilePath, true);
                if (formFile is not null && formFile.Length > 0)
                {
                    var result = Validate(formFile);
                    if (result.Success)
                    {
                        result = Delete(currentFilePath);
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
                            return Success(fileResponse, FilesUpdated);
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
                Logger.LogError($"FileServiceException: {GetType().Name}.Update(Path = {currentFilePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        public Result<FileResponse> Delete(string currentFilePath)
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
                        return Success(fileResponse, FilesDeleted);
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
                Logger.LogError($"FileServiceException: {GetType().Name}.Delete(Path = {currentFilePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        protected Result Validate(IFormFile formFile)
        {
            if (formFile.Length > MaximumSizeInMb * Math.Pow(1024, 2))
                return Error(Culture == Cultures.TR ? $"Geçersiz dosya boyutu, geçerli maksimum dosya boyutu: {MaximumSizeInMb} MB!" :
                    $"Invalid file size, valid maxiumum file size: {MaximumSizeInMb} MB!");
            else if (!Extensions.Contains(GetFileExtension(formFile)))
                return Error(Culture == Cultures.TR ? $"Geçersiz dosya uzantısı, geçerli dosya uzantıları: {string.Join(", ", Extensions)}!" :
                    $"Invalid file extension, valid file extensions: {string.Join(", ", Extensions)}!");
            return Success();
        }

        public Result<List<FileResponse>> Create(List<IFormFile> formFiles)
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
                    validationResult = Validate(formFile);
                    if (!validationResult.Success)
                        break;
                }
                if (validationResult.Success)
                {
                    result = Success(fileResponse);
                    foreach (var formFile in formFiles)
                    {
                        result = Create(formFile);
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
            return Success(fileResponseList, FilesCreated);
        }

        public Result<List<FileResponse>> Delete(List<string> currentFilePaths)
        {
            List<FileResponse> fileResponseList = null;
            FileResponse fileResponse = null;
            Result<FileResponse> result = Success(fileResponse);
            if (currentFilePaths is not null && currentFilePaths.Any())
            {
                fileResponseList = new List<FileResponse>();
                foreach (var currentFilePath in currentFilePaths)
                {
                    result = Delete(currentFilePath);
                    if (!result.Success)
                        break;
                    fileResponseList.Add(result.Data);
                }
                if (!result.Success)
                    return Error(fileResponseList, result);
            }
            return Success(fileResponseList, FilesDeleted);
        }

        public void GetExcel<T>(List<T> list) where T : class, new()
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
                Logger.LogError($"FileServiceException: {GetType().Name}.GetExcel(): {exception.Message}");
            }
        }

        public Result<FileResponse> GetFile(string filePath, bool useOctetStreamContentType = false)
        {
            FileResponse fileResponse = null;
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return Error(fileResponse, HttpStatusCode.NotFound);
                fileResponse = new FileResponse()
                {
                    FileStream = new FileStream(GetFilePath(filePath, true), FileMode.Open),
                    FileContentType = useOctetStreamContentType ? "application/octet-stream" : GetContentType(filePath),
                    FileName = GetFileName(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                Logger.LogError($"FileServiceException: {GetType().Name}.GetFile(Path = {filePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }
    }
}
