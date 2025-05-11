using Microsoft.AspNetCore.Http;
using N4C.App.Services.Files.Models;
using N4C.Extensions;
using OfficeOpenXml;
using System.Net;

namespace N4C.App.Services.Files
{
    public class FileService : Service
    {
        protected FileServiceConfig Config { get; private set; } = new FileServiceConfig();

        private HttpService HttpService { get; }

        public FileService(HttpService httpService, LogService logService) : base(logService)
        {
            HttpService = httpService;
            Config.Culture = HttpService.Culture;
            Set(Config.Culture, Config.TitleTR, Config.TitleEN);
        }

        public void Set(Action<FileServiceConfig> config)
        {
            config.Invoke(Config);
            Set(Config.Culture, Config.TitleTR, Config.TitleEN);
        }

        protected string GetFileExtension(IFormFile formFile)
        {
            return Path.GetExtension(formFile.FileName).ToLower();
        }

        protected string GetFileExtension(string filePath)
        {
            return $".{filePath.Split('.')[1]}";
        }

        protected string GetFilePath(IFormFile formFile)
        {
            return Path.Combine("wwwroot", GetFileFolder(), $"{Guid.NewGuid().ToString()}{GetFileExtension(formFile)}");
        }

        protected string GetFilePath(string filePath, bool wwwroot = false)
        {
            return string.IsNullOrWhiteSpace(filePath) ? null : wwwroot ? $"wwwroot{filePath}" : filePath.Substring(7).Replace(@"\", "/");
        }

        protected string GetFileContentType(string filePath, bool data = false, bool base64 = false)
        {
            var fileExtension = GetFileExtension(filePath);
            var contentType = Config.FileMimeTypes[fileExtension];
            if (data)
                contentType = "data:" + contentType;
            if (base64)
                contentType = contentType + ";base64,";
            return contentType;
        }

        public string GetFileFolder(string filePath = default)
        {
            return string.IsNullOrWhiteSpace(filePath) ? Config.FilesFolder : filePath.Split('/')[1];
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

        public List<string> GetOtherFilePaths(List<FileResponse> otherFiles, int orderInitialValue, int orderPaddingTotalWidth = 3)
        {
            List<string> otherFilePaths = null;
            if (otherFiles is not null && otherFiles.Any())
            {
                otherFilePaths = new List<string>();
                string orderValue;
                for (int i = 0; i < otherFiles.Count; i++)
                {
                    orderValue = orderInitialValue++.ToString().PadLeft(orderPaddingTotalWidth, '0');
                    otherFilePaths.Add($"/{GetFileFolder(otherFiles[i].MainFile)}/{orderValue}/{GetFileName(otherFiles[i].MainFile)}");
                }
            }
            return otherFilePaths;
        }

        public void GetOtherFilePaths(List<string> otherFilePaths)
        {
            if (otherFilePaths is not null && otherFilePaths.Any())
            {
                for (int i = 0; i < otherFilePaths.Count; i++)
                {
                    otherFilePaths[i] = $"/{GetFileFolder(otherFilePaths[i])}/{GetFileName(otherFilePaths[i])}";
                }
            }
        }

        public Result ValidateOtherFiles(List<IFormFile> otherFormFiles, List<string> otherFilePaths = default)
        {
            var otherFilesCount = 0;
            if (otherFormFiles is not null)
                otherFilesCount += otherFormFiles.Count;
            if (otherFilePaths is not null)
                otherFilesCount += otherFilePaths.Count;
            if (otherFilesCount > Config.MaximumOtherFilesCount)
                return Error(Culture == Cultures.TR ? $"Diğer dosya sayısı maksimum {Config.MaximumOtherFilesCount} olmalıdır!" :
                    $"Other files count must be maximum {Config.MaximumOtherFilesCount}!");
            return Success();
        }

        public Result<FileResponse> CreateFile(IFormFile formFile)
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
                LogError($"FileServiceException: {GetType().Name}.CreateFile(formFile): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        public Result<FileResponse> UpdateFile(IFormFile formFile, string filePath)
        {
            FileResponse fileResponse = null;
            try
            {
                filePath = GetFilePath(filePath, true);
                if (formFile is not null && formFile.Length > 0)
                {
                    var result = ValidateFile(formFile);
                    if (result.Success)
                    {
                        result = DeleteFile(filePath);
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
                LogError($"FileServiceException: {GetType().Name}.UpdateFile(formFile, Path = {filePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        public Result<FileResponse> DeleteFile(string filePath)
        {
            FileResponse fileResponse = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = GetFilePath(filePath, true);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        fileResponse = new FileResponse()
                        {
                            MainFile = GetFilePath(filePath)
                        };
                        return Success(fileResponse, Deleted);
                    }
                }
                fileResponse = new FileResponse()
                {
                    MainFile = GetFilePath(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                LogError($"FileServiceException: {GetType().Name}.DeleteFile(Path = {filePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }

        protected Result ValidateFile(IFormFile formFile)
        {
            if (formFile.Length > Config.MaximumFileSizeInMb * Math.Pow(1024, 2))
                return Error(Culture == Cultures.TR ? $"Geçersiz dosya boyutu, geçerli maksimum dosya boyutu: {Config.MaximumFileSizeInMb} MB!" :
                    $"Invalid file size, valid maxiumum file size: {Config.MaximumFileSizeInMb} MB!");
            else if (!Config.FileExtensions.Contains(GetFileExtension(formFile)))
                return Error(Culture == Cultures.TR ? $"Geçersiz dosya uzantısı, geçerli dosya uzantıları: {string.Join(", ", Config.FileExtensions)}!" :
                    $"Invalid file extension, valid file extensions: {string.Join(", ", Config.FileExtensions)}!");
            return Success();
        }

        public Result<List<FileResponse>> CreateFiles(List<IFormFile> formFiles)
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

        public Result<List<FileResponse>> DeleteFiles(List<string> filePaths)
        {
            List<FileResponse> fileResponseList = null;
            FileResponse fileResponse = null;
            Result<FileResponse> result = Success(fileResponse);
            if (filePaths is not null && filePaths.Any())
            {
                fileResponseList = new List<FileResponse>();
                foreach (var filePath in filePaths)
                {
                    result = DeleteFile(filePath);
                    if (!result.Success)
                        break;
                    fileResponseList.Add(result.Data);
                }
                if (!result.Success)
                    return Error(fileResponseList, result);
            }
            return Success(fileResponseList, Deleted);
        }

        public void GetExcel<T>(List<T> list, string fileName = default) where T : class, new()
        {
            try
            {
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace("-", "").Replace(":", "").Replace(" ", "_");
                fileName = !string.IsNullOrWhiteSpace(fileName) ? $"{fileName}.xlsx" : (Culture == Cultures.TR ? $"Rapor_{dateTime}.xlsx" : $"Report_{dateTime}.xlsx");
                var worksheet = Culture == Cultures.TR ? "Sayfa1" : "Sheet1";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                ExcelPackage.LicenseContext = Config.ExcelLicenseCommercial ? LicenseContext.Commercial : LicenseContext.NonCommercial;
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
                LogError($"FileServiceException: {GetType().Name}.GetExcel(list): {exception.Message}");
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
                    FileContentType = useOctetStreamContentType ? "application/octet-stream" : GetFileContentType(filePath),
                    FileName = GetFileName(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                LogError($"FileServiceException: {GetType().Name}.GetFile(Path = {filePath}): {exception.Message}");
                return Error(fileResponse, HttpStatusCode.InternalServerError);
            }
        }
    }
}
