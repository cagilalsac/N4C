using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using N4C.Domain;
using N4C.Extensions;
using N4C.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Globalization;
using System.Net;
using System.Security.Claims;

namespace N4C.Services
{
    public class Service
    {
        protected virtual Config Config { get; set; } = new Config();

        public string Culture => Config.Culture;

        private IHttpContextAccessor HttpContextAccessor { get; }
        private ILogger<Service> Logger { get; }

        public Service(IHttpContextAccessor httpContextAccessor, ILogger<Service> logger)
        {
            HttpContextAccessor = httpContextAccessor;
            Logger = logger;
            Set(null);
        }

        public void Set(string culture, string titleTR = default, string titleEN = default)
        {
            Config.SetCulture(GetCookie(Defaults.Culture) ?? culture);
            Config.SetTitle(titleTR, titleEN);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
        }

        private Result Result(HttpStatusCode httpStatusCode, int? id = default, string message = default, bool modelStateErrors = true)
        {
            return new Result(httpStatusCode, message, Culture, Config.Title, id, modelStateErrors);
        }

        private Result<TData> Result<TData>(HttpStatusCode httpStatusCode, TData data, string message = default, bool modelStateErrors = true,
            Page page = default, Order order = default) where TData : class, new()
        {
            return new Result<TData>(httpStatusCode, data, page, order, message, Culture, Config.Title, modelStateErrors);
        }

        protected Result Result(Result previousResult, string tr = default, string en = default) 
            => Result(previousResult.HttpStatusCode, previousResult.Id, tr is null && en is null ? previousResult.Message : Culture == Defaults.TR ? tr : en,
                previousResult.ModelStateErrors);

        protected Result<TData> Result<TData>(Result previousResult, TData data, string tr = default, string en = default) where TData : class, new()
            => Result(previousResult.HttpStatusCode, data, tr is null && en is null ? previousResult.Message : Culture == Defaults.TR ? tr : en,
                previousResult.ModelStateErrors);

        public virtual Result NotFound(int? id = default) => Result(HttpStatusCode.NotFound, id, Config.NotFound);

        public virtual Result Found(IEnumerable<int> ids) 
            => ids.Count() > 1 ? Result(HttpStatusCode.OK, null, $"{ids.Count()} {Config.Found}") :
                ids.Count() == 1 ? Result(HttpStatusCode.OK, ids.First()) :
                NotFound();

        public virtual Result Success(string tr = default, string en = default, int? id = default)
            => Result(HttpStatusCode.OK, id, string.Empty);

        public virtual Result Error(string tr = default, string en = default, int? id = default)
            => Result(HttpStatusCode.BadRequest, id, $"{Config.Error} {(Culture == Defaults.TR ? tr : en)}".TrimEnd());

        public virtual Result Error(Exception exception, int? id = default)
            => Result(HttpStatusCode.InternalServerError, id, Config.Exception);

        public virtual Result Created(int? id = default) => Result(HttpStatusCode.Created, id, Config.Created);
        public virtual Result Updated(int? id = default) => Result(HttpStatusCode.NoContent, id, Config.Updated);
        public virtual Result Deleted(int? id = default) => Result(HttpStatusCode.NoContent, id, Config.Deleted);
        public virtual Result Unauthorized(int? id = default) => Result(HttpStatusCode.Unauthorized, id, Config.Unauthorized);

        protected virtual Result<List<TData>> Found<TData>(List<TData> list, Page page = default, Order order = default) where TData : Data, new()
            => list.Any() ? Result(HttpStatusCode.OK, list, $"{list.Count} {Config.Found}", false, page, order) : 
                Result(HttpStatusCode.NotFound, list, Config.NotFound);

        protected virtual Result<TData> NotFound<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.NotFound, item, Config.NotFound);

        protected virtual Result<TData> Found<TData>(TData item) where TData : Data, new()
            => item is not null ? Result(HttpStatusCode.OK, item) : NotFound(item);

        protected virtual Result<TData> Success<TData>(TData item, string tr = default, string en = default) where TData : Data, new()
            => Result(HttpStatusCode.OK, item, string.Empty);

        protected virtual Result<TData> Error<TData>(TData item, string tr = default, string en = default) where TData : Data, new()
            => Result(HttpStatusCode.BadRequest, item, $"{Config.Error} {(Culture == Defaults.TR ? tr : en)}".TrimEnd());

        protected virtual Result<List<TData>> Error<TData>(Exception exception, List<TData> list) where TData : Data, new()
        {
            LogError("ServiceException: " + exception.Message);
            return Result(HttpStatusCode.InternalServerError, list, Config.Exception);
        }

        protected virtual Result<TData> Error<TData>(Exception exception, TData item) where TData : Data, new()
        {
            LogError("ServiceException: Id = " + item.Id + ": " + exception.Message);
            return Result(HttpStatusCode.InternalServerError, item, Config.Exception);
        }

        protected virtual Result<TData> Created<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.Created, item, Config.Created);

        protected virtual Result<List<TData>> Created<TData>(List<TData> list) where TData : Data, new()
            => Result(HttpStatusCode.Created, list, Config.Created);

        protected virtual Result<TData> Updated<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.NoContent, item, Config.Updated);

        protected virtual Result<TData> Deleted<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.NoContent, item, Config.Deleted);

        protected virtual Result<List<TData>> Deleted<TData>(List<TData> list) where TData : Data, new()
            => Result(HttpStatusCode.NoContent, list, Config.Deleted);

        protected virtual Result<TData> RelationsFound<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.BadRequest, item, $"{Config.Error} {Config.RelationsFound}");

        protected virtual Result<TData> Validated<TData>(TData item, string modelStateErrors, string uniquePropertyError = default) where TData : Data, new()
        {
            var error = modelStateErrors.Length > 0 || (uniquePropertyError ?? string.Empty).Length > 0;
            var errors = Config.Error;
            if (modelStateErrors.Length > 0)
                errors += ";" + modelStateErrors;
            errors += ";" + uniquePropertyError;
            return error ? Result(HttpStatusCode.BadRequest, item, errors.Trim(';'), Config.ModelStateErrors) : Result(HttpStatusCode.OK, item);
        }

        public Result Validate(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors(Culture);
            if (errors.Any())
                return Result(HttpStatusCode.BadRequest, null, string.Join(";", errors));
            return Result(HttpStatusCode.OK);
        }

        public void LogError(string message) => Logger.LogError(message);

        public string GetUserName() => HttpContextAccessor.HttpContext?.User.Identity?.Name;

        public int GetUserId() => Convert.ToInt32(HttpContextAccessor.HttpContext?.User.Claims?.SingleOrDefault(claim => claim.Type == nameof(Data.Id))?.Value);

        public T GetSession<T>(string key) where T : class
        {
            return JsonConvert.DeserializeObject<T>(HttpContextAccessor.HttpContext.Session.GetString(key));
        }

        public void CreateSession<T>(string key, T instance) where T : class
        {
            HttpContextAccessor.HttpContext.Session.SetString(key, JsonConvert.SerializeObject(instance));
        }

        public void DeleteSession(string key)
        {
            HttpContextAccessor.HttpContext.Session.Remove(key);
        }

        public string GetCookie(string key)
        {
            return HttpContextAccessor.HttpContext.Request.Cookies[key];
        }

        public void CreateCookie(string key, string value, CookieOptions cookieOptions)
        {
            HttpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }

        public void CreateCookie(string key, string value, bool isHttpOnly = true, int? expirationInMinutes = default)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = expirationInMinutes.HasValue ?
                    DateTime.SpecifyKind(DateTime.Now.AddMinutes(expirationInMinutes.Value), DateTimeKind.Utc) : DateTimeOffset.MaxValue,
                HttpOnly = isHttpOnly
            };
            CreateCookie(key, value, cookieOptions);
        }

        public void DeleteCookie(string key, bool isHttpOnly = true)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Utc),
                HttpOnly = isHttpOnly
            };
            CreateCookie(key, string.Empty, cookieOptions);
        }

        protected virtual List<Claim> GetClaims(int userId, string userName, IEnumerable<string> roleNames)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(nameof(Data.Id), userId.ToString())
            };
            foreach (var roleName in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }
            return claims;
        }

        protected async Task SignIn(List<Claim> claims, DateTime? expiration = default, bool isPersistent = true, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            var identity = new ClaimsIdentity(claims, authenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authenticationProperties = new AuthenticationProperties() { IsPersistent = isPersistent };
            if (expiration.HasValue)
                authenticationProperties.ExpiresUtc = DateTime.SpecifyKind(expiration.Value, DateTimeKind.Utc);
            await HttpContextAccessor.HttpContext.SignInAsync(authenticationScheme, principal, authenticationProperties);
        }

        protected async Task SignOut(string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            await HttpContextAccessor.HttpContext.SignOutAsync(authenticationScheme);
        }

        public void GetResponse(byte[] data, string fileName, string contentType)
        {
            if (data is not null && data.Length > 0)
            {
                HttpContextAccessor.HttpContext.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO = true;
                HttpContextAccessor.HttpContext.Response.Headers.Clear();
                HttpContextAccessor.HttpContext.Response.Clear();
                HttpContextAccessor.HttpContext.Response.ContentType = contentType;
                HttpContextAccessor.HttpContext.Response.Headers.Append("content-length", data.Length.ToString());
                HttpContextAccessor.HttpContext.Response.Headers.Append("content-disposition", "attachment; filename=\"" + fileName + "\"");
                HttpContextAccessor.HttpContext.Response.Body.WriteAsync(data, 0, data.Length);
                HttpContextAccessor.HttpContext.Response.Body.Flush();
            }
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

        protected string GetFileFolder(string filePath = default)
        {
            return string.IsNullOrWhiteSpace(filePath) ? Config.FilesFolder : filePath.Split('/')[1];
        }

        protected string GetFileName(string filePath, bool extension = true)
        {
            var fileName = filePath.Split('/')[filePath.Split('/').Length - 1].Split('.')[0];
            if (extension)
                fileName += GetFileExtension(filePath);
            return fileName;
        }

        protected int GetFileOrder(string filePath)
        {
            return Convert.ToInt32(filePath.Split('/')[2]);
        }

        protected List<string> GetOtherFilePaths(List<FileResponse> otherFiles, int orderInitialValue, int orderPaddingTotalWidth = 3)
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

        protected void GetOtherFilePaths(List<string> otherFilePaths)
        {
            if (otherFilePaths is not null && otherFilePaths.Any())
            {
                for (int i = 0; i < otherFilePaths.Count; i++)
                {
                    otherFilePaths[i] = $"/{GetFileFolder(otherFilePaths[i])}/{GetFileName(otherFilePaths[i])}";
                }
            }
        }

        protected virtual Result ValidateFile(IFormFile formFile)
        {
            if (formFile.Length > Config.MaximumFileSizeInMb * Math.Pow(1024, 2))
            {
                return Error(Culture == Defaults.TR ? $"Geçersiz dosya boyutu, geçerli maksimum dosya boyutu: {Config.MaximumFileSizeInMb} MB!" :
                    $"Invalid file size, valid maxiumum file size: {Config.MaximumFileSizeInMb} MB!");
            }
            else if (!Config.FileExtensions.Contains(GetFileExtension(formFile)))
            {
                return Error(Culture == Defaults.TR ? $"Geçersiz dosya uzantısı, geçerli dosya uzantıları: {string.Join(", ", Config.FileExtensions)}!" :
                    $"Invalid file extension, valid file extensions: {string.Join(", ", Config.FileExtensions)}!");
            }
            return Success();
        }

        protected virtual Result ValidateOtherFiles(List<IFormFile> otherFormFiles, List<string> otherFilePaths = default)
        {
            var otherFilesCount = 0;
            if (otherFormFiles is not null)
                otherFilesCount += otherFormFiles.Count;
            if (otherFilePaths is not null)
                otherFilesCount += otherFilePaths.Count;
            if (otherFilesCount > Config.MaximumOtherFilesCount)
                return Error(Culture == Defaults.TR ? $"Diğer dosya sayısı maksimum {Config.MaximumOtherFilesCount} olmalıdır!" :
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
                        return Created(fileResponse);
                    }
                    return Result(result, fileResponse);
                }
                fileResponse = new FileResponse()
                {
                    MainFile = GetFilePath(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                return Error(exception, fileResponse);
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
                        return Deleted(fileResponse);
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
                return Error(exception, fileResponse);
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
                            return Updated(fileResponse);
                        }
                    }
                    return Result(result, fileResponse);
                }
                fileResponse = new FileResponse()
                {
                    MainFile = GetFilePath(filePath)
                };
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                return Error(exception, fileResponse);
            }
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
                        return Result(result, fileResponseList);
                }
                else
                {
                    return Result(validationResult, fileResponseList);
                }
            }
            return Created(fileResponseList);
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
                    return Result(result, fileResponseList);
            }
            return Deleted(fileResponseList);
        }

        public void GetExcel<T>(List<T> list, string fileName = default) where T : class, new()
        {
            try
            {
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace("-", "").Replace(":", "").Replace(" ", "_");
                fileName = !string.IsNullOrWhiteSpace(fileName) ? $"{fileName}.xlsx" : (Culture == Defaults.TR ? $"Rapor_{dateTime}.xlsx" : $"Report_{dateTime}.xlsx");
                var worksheet = Culture == Defaults.TR ? "Sayfa1" : "Sheet1";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                ExcelPackage.LicenseContext = Config.ExcelLicenseCommercial ? LicenseContext.Commercial : LicenseContext.NonCommercial;
                var excelPackage = new ExcelPackage();
                var excelWorksheet = excelPackage.Workbook.Worksheets.Add(worksheet);
                excelWorksheet.Cells["A1"].LoadFromDataTable(list.ConvertToDataTable(Culture), true);
                excelWorksheet.Cells["A1:AZ1"].Style.Font.Bold = true;
                excelWorksheet.Cells["A1:AZ1"].AutoFilter = true;
                excelWorksheet.Cells["A:AZ"].AutoFitColumns();
                GetResponse(excelPackage.GetAsByteArray(), fileName, contentType);
            }
            catch (Exception exception)
            {
                LogError("ServiceException: " + exception.Message);
            }
        }

        public Result<FileResponse> GetFile(string filePath, bool useOctetStreamContentType = false)
        {
            FileResponse fileResponse = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    fileResponse = new FileResponse()
                    {
                        FileStream = new FileStream(GetFilePath(filePath, true), FileMode.Open),
                        FileContentType = useOctetStreamContentType ? "application/octet-stream" : GetFileContentType(filePath),
                        FileName = GetFileName(filePath)
                    };
                }
                return Found(fileResponse);
            }
            catch (Exception exception)
            {
                return Error(exception, fileResponse);
            }
        }
    }
}
