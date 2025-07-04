using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using N4C.Domain;
using N4C.Extensions;
using N4C.Models;
using OfficeOpenXml;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace N4C.Services
{
    public class Service
    {
        protected virtual ServiceConfig Config { get; set; } = new ServiceConfig();

        protected JsonSerializerOptions JsonSerializerOptions { get; private set; }

        public string Culture => Config.Culture;
        public string TitleTR => Config.TitleTR;
        public string TitleEN => Config.TitleEN;
        protected string NotFound => Config.NotFound;
        protected string RelationsFound => Config.RelationsFound;

        private bool _api;

        private IHttpContextAccessor HttpContextAccessor { get; }
        private IHttpClientFactory HttpClientFactory { get; }
        private ILogger<Service> Logger { get; }

        public Service(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ILogger<Service> logger)
        {
            HttpContextAccessor = httpContextAccessor;
            HttpClientFactory = httpClientFactory;
            Logger = logger;
            JsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            Set();
        }

        internal void Set(string culture = default, string titleTR = default, string titleEN = default)
        {
            Config.SetCulture(_api ? culture.HasNotAny(Settings.Culture) : culture.HasNotAny(GetCookie(".N4C.Culture")));
            Config.SetTitle(titleTR, titleEN);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
        }

        internal void SetApi(bool api)
        {
            _api = api;
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

        public Result Result(Exception exception)
        {
            LogError("ServiceException: " + exception.Message);
            return Result(HttpStatusCode.InternalServerError, null, Config.Exception, false);
        }

        protected Result Result(Result previousResult, string tr = default, string en = default)
            => Result(previousResult.HttpStatusCode, previousResult.Id, tr is null && en is null ? previousResult.Message : Culture == Defaults.TR ? tr : en,
                previousResult.ModelStateErrors);

        protected Result Result(HttpResponseMessage httpResponseMessage, int? id = default)
        {
            var message = httpResponseMessage.StatusCode == HttpStatusCode.NotFound ? Config.NotFound :
                httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized ? Config.Unauthorized :
                httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError ? Config.Exception :
                httpResponseMessage.StatusCode == HttpStatusCode.OK || httpResponseMessage.StatusCode == HttpStatusCode.BadRequest ? 
                JsonSerializer.Deserialize<Result>(httpResponseMessage.Content.ReadAsStringAsync().Result, JsonSerializerOptions)?.Message : string.Empty;
            return Result(httpResponseMessage.StatusCode, id, message);
        }

        protected Result<TData> Result<TData>(Exception exception, TData data) where TData : class, new()
        {
            LogError("ServiceException: " + exception.Message);
            return Result<TData>(HttpStatusCode.InternalServerError, null, Config.Exception, false);
        }

        public Result<TData> Result<TData>(Result previousResult, TData data, string tr = default, string en = default) where TData : class, new()
            => Result(previousResult.HttpStatusCode, data, tr is null && en is null ? previousResult.Message : Culture == Defaults.TR ? tr : en,
                previousResult.ModelStateErrors);

        protected Result<List<TData>> Result<TData>(HttpResponseMessage httpResponseMessage, List<TData> list) where TData : class, new()
        {
            var message = httpResponseMessage.StatusCode == HttpStatusCode.NotFound ? Config.NotFound :
                httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized ? Config.Unauthorized :
                httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError ? Config.Exception :
                httpResponseMessage.StatusCode == HttpStatusCode.OK || httpResponseMessage.StatusCode == HttpStatusCode.BadRequest ?
                JsonSerializer.Deserialize<Result>(httpResponseMessage.Content.ReadAsStringAsync().Result, JsonSerializerOptions)?.Message : string.Empty;
            return Result(httpResponseMessage.StatusCode, list, message);
        }

        protected Result<TData> Result<TData>(HttpResponseMessage httpResponseMessage, TData item) where TData : class, new()
        {
            var message = httpResponseMessage.StatusCode == HttpStatusCode.NotFound ? Config.NotFound :
                httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized ? Config.Unauthorized :
                httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError ? Config.Exception :
                httpResponseMessage.StatusCode == HttpStatusCode.OK || httpResponseMessage.StatusCode == HttpStatusCode.BadRequest ?
                JsonSerializer.Deserialize<Result>(httpResponseMessage.Content.ReadAsStringAsync().Result, JsonSerializerOptions)?.Message : string.Empty;
            return Result(httpResponseMessage.StatusCode, item, message);
        }

        public virtual Result Success(string tr = default, string en = default, int? id = default)
            => Result(HttpStatusCode.OK, id, string.Empty);

        public virtual Result Error(string tr = default, string en = default, int? id = default)
            => Result(HttpStatusCode.BadRequest, id, $"{Config.Error} {(Culture == Defaults.TR ? tr : en)}".TrimEnd());

        public virtual Result Created(int? id = default, string message = default) => Result(HttpStatusCode.Created, id, message.HasNotAny(Config.Created));
        public virtual Result Updated(int? id = default, string message = default) => Result(HttpStatusCode.NoContent, id, message.HasNotAny(Config.Updated));
        public virtual Result Deleted(int? id = default, string message = default) => Result(HttpStatusCode.NoContent, id, message.HasNotAny(Config.Deleted));

        protected virtual Result<List<TData>> Success<TData>(List<TData> list) where TData : Data, new()
            => list.HasAny() ? Result(HttpStatusCode.OK, list, $"{list.Count} {Config.Found}", false) :
                Result(HttpStatusCode.NotFound, list, Config.NotFound);

        protected virtual Result<List<TData>> Success<TData>(List<TData> list, Page page, Order order) where TData : Data, new()
            => page.TotalRecordsCount > 0 ? Result(HttpStatusCode.OK, list, $"{page.TotalRecordsCount} {Config.Found}", false, page, order) :
                Result(HttpStatusCode.NotFound, list, Config.NotFound);

        protected virtual Result<TData> Success<TData>(TData item) where TData : Data, new()
            => Result(item is null ? HttpStatusCode.NotFound : HttpStatusCode.OK, item, item is null ? Config.NotFound : string.Empty);

        protected virtual Result<List<TData>> Error<TData>(List<TData> list, string tr, string en) where TData : Data, new()
            => Result(HttpStatusCode.BadRequest, list, $"{Config.Error} {(Culture == Defaults.TR ? tr : en)}".TrimEnd());

        protected virtual Result<TData> Error<TData>(TData item, string tr, string en) where TData : Data, new()
            => Result(HttpStatusCode.BadRequest, item, $"{Config.Error} {(Culture == Defaults.TR ? tr : en)}".TrimEnd());

        protected virtual Result<TData> Error<TData>(TData item, string message) where TData : Data, new()
            => Result(message == NotFound ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest, item, $"{Config.Error} {message}");

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

        protected virtual Result<TData> Validated<TData>(TData item, string modelStateErrors, string uniquePropertyError = default) where TData : Data, new()
        {
            var error = modelStateErrors.HasAny() || uniquePropertyError.HasAny();
            var errors = Config.Error;
            if (uniquePropertyError.HasAny())
                errors += " " + uniquePropertyError;
            if (modelStateErrors.HasAny())
                errors += ";" + modelStateErrors;
            return error ? Result(HttpStatusCode.BadRequest, item, errors.Trim(';'), Config.ModelStateErrors) : Result(HttpStatusCode.OK, item);
        }

        public Result Validate(ModelStateDictionary modelState)
        {
            var errors = modelState?.GetErrors(Culture);
            if (errors.HasAny())
                return Result(HttpStatusCode.BadRequest, null, string.Join(";", errors));
            return Result(HttpStatusCode.OK);
        }

        public void LogError(string message) => Logger.LogError(message);

        public string GetUserName() => HttpContextAccessor.HttpContext?.User.Identity?.Name;

        public int GetUserId() => Convert.ToInt32(HttpContextAccessor.HttpContext?.User.Claims?.SingleOrDefault(claim => claim.Type == nameof(Data.Id))?.Value);

        public int GetUserId(List<Claim> claims) => Convert.ToInt32(claims.SingleOrDefault(claim => claim.Type == nameof(Data.Id))?.Value);

        public T GetSession<T>(string key) where T : class
        {
            if (_api || Settings.SessionExpirationInMinutes <= 0)
                return null;
            var value = HttpContextAccessor.HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(value))
                return null;
            return JsonSerializer.Deserialize<T>(value);
        }

        public void CreateSession<T>(string key, T instance) where T : class
        {
            if (!_api && Settings.SessionExpirationInMinutes > 0)
            {
                var value = JsonSerializer.Serialize(instance);
                HttpContextAccessor.HttpContext.Session.SetString(key, value);
            }
        }

        public void DeleteSession(string key)
        {
            if (!_api && Settings.SessionExpirationInMinutes > 0)
                HttpContextAccessor.HttpContext.Session.Remove(key);
        }

        public string GetCookie(string key)
        {
            if (_api)
                return null;
            return HttpContextAccessor.HttpContext.Request.Cookies[key];
        }

        public void CreateCookie(string key, string value, CookieOptions cookieOptions)
        {
            if (!_api)
                HttpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }

        public void CreateCookie(string key, string value, bool httpOnly = true, int? expirationInMinutes = default)
        {
            if (!_api)
            {
                var cookieOptions = new CookieOptions()
                {
                    Expires = expirationInMinutes.HasValue ?
                        DateTime.SpecifyKind(DateTime.Now.AddMinutes(expirationInMinutes.Value), DateTimeKind.Utc) : DateTimeOffset.MaxValue,
                    HttpOnly = httpOnly
                };
                CreateCookie(key, value, cookieOptions);
            }
        }

        public void DeleteCookie(string key, bool httpOnly = true)
        {
            if (!_api)
            {
                var cookieOptions = new CookieOptions()
                {
                    Expires = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Utc),
                    HttpOnly = httpOnly
                };
                CreateCookie(key, string.Empty, cookieOptions);
            }
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

        protected virtual List<Claim> GetClaims(string token)
        {
            SecurityToken securityToken;
            if (token.StartsWith(JwtBearerDefaults.AuthenticationScheme))
                token = token.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length).TrimStart();
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = Settings.JwtSigningKey
            };
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var principal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            return securityToken is null ? null : principal?.Claims?.ToList();
        }

        protected async Task Login(List<Claim> claims, DateTime? expiration = default, bool isPersistent = true, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            var identity = new ClaimsIdentity(claims, authenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authenticationProperties = new AuthenticationProperties() { IsPersistent = isPersistent };
            if (expiration.HasValue)
                authenticationProperties.ExpiresUtc = DateTime.SpecifyKind(expiration.Value, DateTimeKind.Utc);
            await HttpContextAccessor.HttpContext.SignInAsync(authenticationScheme, principal, authenticationProperties);
        }

        public async Task Logout(string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            await HttpContextAccessor.HttpContext.SignOutAsync(authenticationScheme);
            DeleteSession(nameof(Page));
            DeleteSession(nameof(Order));
            DeleteCookie(".N4C.Token");
            DeleteCookie(".N4C.RefreshToken");
        }

        protected void GetPageOrderSession(PageOrderRequest pageOrderRequest)
        {
            if (pageOrderRequest.PageOrderSession)
            {
                var pageFromSession = GetSession<Page>(nameof(Page));
                if (pageFromSession is not null)
                {
                    pageOrderRequest.Page.Number = pageFromSession.Number;
                    pageOrderRequest.Page.RecordsPerPageCount = pageFromSession.RecordsPerPageCount;
                }
                var orderFromSession = GetSession<Order>(nameof(Order));
                if (orderFromSession is not null)
                {
                    pageOrderRequest.OrderExpression = orderFromSession.Expression;
                }
            }
        }

        protected void SetPageOrderSession(Page page, Order order)
        {
            CreateSession(nameof(Page), page);
            CreateSession(nameof(Order), order);
        }

        protected void GetResponse(byte[] data, string fileName, string contentType)
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
            return filePath.HasNotAny() ? null : wwwroot ? $"wwwroot{filePath}" : filePath.Substring(7).Replace(@"\", "/");
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
            return filePath.HasNotAny() ? Config.FilesFolder : filePath.Split('/')[1];
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
            if (otherFiles.HasAny())
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
            if (otherFilePaths.HasAny())
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
                return Result(exception, fileResponse);
            }
        }

        public Result<FileResponse> DeleteFile(string filePath)
        {
            FileResponse fileResponse = null;
            try
            {
                if (filePath.HasAny())
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
                return Result(exception, fileResponse);
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
                return Result(exception, fileResponse);
            }
        }

        public Result<List<FileResponse>> CreateFiles(List<IFormFile> formFiles)
        {
            List<FileResponse> fileResponseList = null;
            FileResponse fileResponse = null;
            Result<FileResponse> result;
            Result validationResult = Success();
            if (formFiles.HasAny())
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
            if (filePaths.HasAny())
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
                fileName = fileName.HasAny() ? $"{fileName}.xlsx" : (Culture == Defaults.TR ? $"Rapor_{dateTime}.xlsx" : $"Report_{dateTime}.xlsx");
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
                if (filePath.HasAny())
                {
                    fileResponse = new FileResponse()
                    {
                        FileStream = new FileStream(GetFilePath(filePath, true), FileMode.Open),
                        FileContentType = useOctetStreamContentType ? "application/octet-stream" : GetFileContentType(filePath),
                        FileName = GetFileName(filePath)
                    };
                }
                return Success(fileResponse);
            }
            catch (Exception exception)
            {
                return Result(exception, fileResponse);
            }
        }

        protected string GetToken(List<Claim> claims, DateTime? expiration = default)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, Settings.JwtSecurityAlgorithm);
            var jwtSecurityToken = new JwtSecurityToken(Settings.JwtIssuer, Settings.JwtAudience, claims, DateTime.Now, expiration, signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            return jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);
        }

        protected string GetToken()
        {
            var token = HttpContextAccessor.HttpContext?.Request?.Headers?.Authorization.FirstOrDefault().HasNotAny(GetCookie(".N4C.Token")).HasNotAny(string.Empty);
            if (token.StartsWith(JwtBearerDefaults.AuthenticationScheme))
                token = token.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length).TrimStart();
            return token;
        }

        protected string GetRefreshToken(bool cookie = false)
        {
            if (cookie)
                return GetCookie(".N4C.RefreshToken");
            var bytes = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        protected HttpClient CreateHttpClient()
        {
            return HttpClientFactory.CreateClient();
        }

        protected HttpClient CreateHttpClient(string token)
        {
            var httpClient = CreateHttpClient();
            if (token.HasAny())
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
            return httpClient;
        }

        public async Task<Result<TokenResponse>> GetToken(Uri tokenUri, string userName, string password, CancellationToken cancellationToken = default)
        {
            TokenResponse response = null;
            try
            {
                Result<TokenResponse> result;
                Set(tokenUri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var httpResponseMessage = await CreateHttpClient().PostAsJsonAsync(tokenUri,
                    new TokenRequest() { UserName = userName, Password = password }, cancellationToken);
                if (!httpResponseMessage.IsSuccessStatusCode)
                    return Result(httpResponseMessage, response);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                {
                    result = JsonSerializer.Deserialize<Result<TokenResponse>>(httpResponse, JsonSerializerOptions);
                    response = result.Data;
                }
                else
                {
                    response = JsonSerializer.Deserialize<TokenResponse>(httpResponse, JsonSerializerOptions);
                    result = Result(httpResponseMessage, response);
                }
                if (result.Success)
                {
                    CreateCookie(".N4C.Token", response.Token);
                    CreateCookie(".N4C.RefreshToken", response.RefreshToken);
                }
                return result;
            }
            catch (Exception exception)
            {
                return Result(exception, response);
            }
        }

        public async Task<Result<TokenResponse>> GetRefreshToken(Uri refreshTokenUri, string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            TokenResponse response = null;
            try
            {
                Result<TokenResponse> result;
                Set(refreshTokenUri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var httpResponseMessage = await CreateHttpClient().PostAsJsonAsync(refreshTokenUri,
                    new RefreshTokenRequest() { RefreshToken = refreshToken, Token = token }, cancellationToken);
                if (!httpResponseMessage.IsSuccessStatusCode)
                    return Result(httpResponseMessage, response);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                {
                    result = JsonSerializer.Deserialize<Result<TokenResponse>>(httpResponse, JsonSerializerOptions);
                    response = result.Data;
                }
                else
                {
                    response = JsonSerializer.Deserialize<TokenResponse>(httpResponse, JsonSerializerOptions);
                    result = Result(httpResponseMessage, response);
                }
                if (result.Success)
                {
                    CreateCookie(".N4C.Token", response.Token);
                    CreateCookie(".N4C.RefreshToken", response.RefreshToken);
                }
                return result;
            }
            catch (Exception exception)
            {
                return Result(exception, response);
            }
        }

        protected async Task<Result<TData>> GetResponse<TData>(HttpResponseMessage httpResponseMessage, TData data, CancellationToken cancellationToken = default) where TData : class, new()
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
                return Result(httpResponseMessage, data);
            var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            if (httpResponse.Contains(typeof(Result).ToString()))
                return JsonSerializer.Deserialize<Result<TData>>(httpResponse, JsonSerializerOptions);
            return Result(httpResponseMessage, JsonSerializer.Deserialize<TData>(httpResponse, JsonSerializerOptions));
        }

        public virtual async Task<Result<List<TResponse>>> GetResponse<TResponse>(Uri uri, string token = default, CancellationToken cancellationToken = default)
           where TResponse : class, new()
        {
            if (uri is null)
                return null;
            List<TResponse> list = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var httpResponseMessage = await CreateHttpClient(token.HasNotAny(GetToken())).GetAsync(uri.AbsoluteUri, cancellationToken);
                return await GetResponse(httpResponseMessage, list, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, list);
            }
        }

        public virtual async Task<Result<List<TResponse>>> GetResponse<TResponse>(Uri uri, PageOrderRequest pageOrderRequest, string token = default, CancellationToken cancellationToken = default)
            where TResponse : class, new()
        {
            if (uri is null)
                return null;
            List<TResponse> list = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                GetPageOrderSession(pageOrderRequest);
                var httpResponseMessage = await CreateHttpClient(token.HasNotAny(GetToken()))
                    .GetAsync($"{uri.AbsoluteUri}&pageNumber={pageOrderRequest.Page.Number}&recordsPerPageCount={pageOrderRequest.Page.RecordsPerPageCount}&orderExpression={pageOrderRequest.OrderExpression}", cancellationToken);
                SetPageOrderSession(pageOrderRequest.Page, new Order() { Expression = pageOrderRequest.OrderExpression });
                return await GetResponse(httpResponseMessage, list, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, list);
            }
        }

        public async Task<Result<List<TResponse>>> GetResponse<TResponse>(Uri uri, Uri refreshTokenUri, PageOrderRequest pageOrderRequest, CancellationToken cancellationToken = default)
            where TResponse : class, new()
        {
            if (uri is null)
                return null;
            List<TResponse> list = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var token = GetToken();
                GetPageOrderSession(pageOrderRequest);
                var httpResponseMessage = await CreateHttpClient(token)
                    .GetAsync($"{uri.AbsoluteUri}&pageNumber={pageOrderRequest.Page.Number}&recordsPerPageCount={pageOrderRequest.Page.RecordsPerPageCount}&orderExpression={pageOrderRequest.OrderExpression}", cancellationToken);
                if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && refreshTokenUri is not null)
                {
                    var tokenResult = await GetRefreshToken(refreshTokenUri, token, GetRefreshToken(true), cancellationToken);
                    if (!tokenResult.Success)
                        return Result(tokenResult, list, "Lütfen çıkış yapıp tekrar giriş yapınız", "Please logout and login again");
                    token = tokenResult.Data.Token;
                    return await GetResponse<TResponse>(uri, pageOrderRequest, token, cancellationToken);
                }
                SetPageOrderSession(pageOrderRequest.Page, new Order() { Expression = pageOrderRequest.OrderExpression });
                return await GetResponse(httpResponseMessage, list, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, list);
            }
        }

        public async Task<Result<List<TResponse>>> GetResponse<TResponse>(string uri, string token = default, CancellationToken cancellationToken = default)
            where TResponse : class, new()
        {
            return await GetResponse<TResponse>(uri.GetUri(UriKind.Absolute), token, cancellationToken);
        }

        public async Task<Result<List<TResponse>>> GetResponse<TResponse>(Uri uri, Uri refreshTokenUri, CancellationToken cancellationToken = default)
           where TResponse : class, new()
        {
            if (uri is null)
                return null;
            List<TResponse> list = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var token = GetToken();
                var httpResponseMessage = await CreateHttpClient(token).GetAsync(uri.AbsoluteUri, cancellationToken);
                if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && refreshTokenUri is not null)
                {
                    var tokenResult = await GetRefreshToken(refreshTokenUri, token, GetRefreshToken(true), cancellationToken);
                    if (!tokenResult.Success)
                        return Result(tokenResult, list, "Lütfen çıkış yapıp tekrar giriş yapınız", "Please logout and login again");
                    token = tokenResult.Data.Token;
                    return await GetResponse<TResponse>(uri, token, cancellationToken);
                }
                return await GetResponse(httpResponseMessage, list, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, list);
            }
        }

        public virtual async Task<Result<TResponse>> GetResponse<TResponse>(Uri uri, int id, string token = default, CancellationToken cancellationToken = default)
            where TResponse : class, new()
        {
            if (uri is null)
                return null;
            TResponse item = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var httpResponseMessage = await CreateHttpClient(token).GetAsync($"{uri.GetLeftPart(UriPartial.Path)}/{id}{uri.Query}", cancellationToken);
                return await GetResponse(httpResponseMessage, item, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, item);
            }
        }

        public async Task<Result<TResponse>> GetResponse<TResponse>(Uri uri, Uri refreshTokenUri, int id, CancellationToken cancellationToken = default)
            where TResponse : class, new()
        {
            if (uri is null)
                return null;
            TResponse item = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var token = GetToken();
                var httpResponseMessage = await CreateHttpClient(token).GetAsync($"{uri.GetLeftPart(UriPartial.Path)}/{id}{uri.Query}", cancellationToken);
                if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && refreshTokenUri is not null)
                {
                    var tokenResult = await GetRefreshToken(refreshTokenUri, token, GetRefreshToken(true), cancellationToken);
                    if (!tokenResult.Success)
                        return Result(tokenResult, item, "Lütfen çıkış yapıp tekrar giriş yapınız", "Please logout and login again");
                    token = tokenResult.Data.Token;
                    return await GetResponse<TResponse>(uri, id, token, cancellationToken);
                }
                return await GetResponse(httpResponseMessage, item, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, item);
            }
        }

        public virtual async Task<Result<TRequest>> Create<TRequest>(Uri uri, TRequest request, string token = default, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (uri is null)
                return null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var httpResponseMessage = await CreateHttpClient(token).PostAsJsonAsync(uri, request, cancellationToken);
                return await GetResponse(httpResponseMessage, request, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public async Task<Result<TRequest>> Create<TRequest>(Uri uri, Uri refreshTokenUri, TRequest request, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (uri is null)
                return null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var token = GetToken();
                var httpResponseMessage = await CreateHttpClient(token).PostAsJsonAsync(uri, request, cancellationToken);
                if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && refreshTokenUri is not null)
                {
                    var tokenResult = await GetRefreshToken(refreshTokenUri, token, GetRefreshToken(true), cancellationToken);
                    if (!tokenResult.Success)
                        return Result(tokenResult, request, "Lütfen çıkış yapıp tekrar giriş yapınız", "Please logout and login again");
                    token = tokenResult.Data.Token;
                    return await Create(uri, request, token, cancellationToken);
                }
                return await GetResponse(httpResponseMessage, request, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public virtual async Task<Result<TRequest>> Update<TRequest>(Uri uri, TRequest request, string token = default, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (uri is null)
                return null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var httpResponseMessage = await CreateHttpClient(token).PutAsJsonAsync(uri, request, cancellationToken);
                return await GetResponse(httpResponseMessage, request, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public async Task<Result<TRequest>> Update<TRequest>(Uri uri, Uri refreshTokenUri, TRequest request, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (uri is null)
                return null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var token = GetToken();
                var httpResponseMessage = await CreateHttpClient(token).PutAsJsonAsync(uri, request, cancellationToken);
                if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && refreshTokenUri is not null)
                {
                    var tokenResult = await GetRefreshToken(refreshTokenUri, token, GetRefreshToken(true), cancellationToken);
                    if (!tokenResult.Success)
                        return Result(tokenResult, request, "Lütfen çıkış yapıp tekrar giriş yapınız", "Please logout and login again");
                    token = tokenResult.Data.Token;
                    return await Update(uri, request, token, cancellationToken);
                }
                return await GetResponse(httpResponseMessage, request, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public virtual async Task<Result<TRequest>> Delete<TRequest>(Uri uri, int id, string token = default, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (uri is null)
                return null;
            TRequest request = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var httpResponseMessage = await CreateHttpClient(token).DeleteAsync($"{uri.GetLeftPart(UriPartial.Path)}/{id}{uri.Query}", cancellationToken);
                return await GetResponse(httpResponseMessage, request, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public async Task<Result<TRequest>> Delete<TRequest>(Uri uri, Uri refreshTokenUri, int id, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (uri is null)
                return null;
            TRequest request = null;
            try
            {
                Set(uri.AbsoluteUri.GetQueryStringValue(nameof(Culture)), TitleTR, TitleEN);
                var token = GetToken();
                var httpResponseMessage = await CreateHttpClient(token).DeleteAsync($"{uri.GetLeftPart(UriPartial.Path)}/{id}{uri.Query}", cancellationToken);
                if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && refreshTokenUri is not null)
                {
                    var tokenResult = await GetRefreshToken(refreshTokenUri, token, GetRefreshToken(true), cancellationToken);
                    if (!tokenResult.Success)
                        return Result(tokenResult, request, "Lütfen çıkış yapıp tekrar giriş yapınız", "Please logout and login again");
                    token = tokenResult.Data.Token;
                    return await Delete<TRequest>(uri, id, token, cancellationToken);
                }
                return await GetResponse(httpResponseMessage, request, cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }
    }
}
