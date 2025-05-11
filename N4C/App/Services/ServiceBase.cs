using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Extensions;
using System.Globalization;
using System.Net;

namespace N4C.App.Services
{
    public abstract class Service
    {
        public string Culture { get; private set; }
        public string Title { get; private set; }

        public string Failed { get; set; }
        public string Successful { get; set; }
        public string Exception { get; set; }
        public string Unauthorized { get; set; }
        public string NotFound { get; set; }
        public string Found { get; set; }
        public string Created { get; set; }
        public string Updated { get; set; }
        public string Deleted { get; set; }

        private LogService LogService { get; }

        protected Service(LogService logService)
        {
            LogService = logService;
            Set(Settings.Culture, "Kayıt", "Record");
        }

        public void Set(string culture, string titleTR, string titleEN)
        {
            Culture = culture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
            Failed = Culture == Cultures.TR ? "İşlem gerçekleştirilemedi!" : "Operation failed!";
            Successful = Culture == Cultures.TR ? "İşlem başarıyla gerçekleştirildi." : "Operation successful.";
            Exception = Culture == Cultures.TR ? "Hata meydana geldi!" : "Exception occurred!";
            Unauthorized = Culture == Cultures.TR ? "Yetkisiz işlem!" : "Unauthorized operation!";
            Title = Culture == Cultures.TR ? titleTR : titleEN ?? "Record";
            NotFound = Culture == Cultures.TR ? $"{Title} bulunamadı" : $"{Title} not found";
            Found = Culture == Cultures.TR ? (Title == "Kayıt" ? $"{Title.ToLower()} bulundu" : $"{Title.ToLower()} kaydı bulundu") :
                (Title == "Record" ? $"{Title.ToLower()}(s) found" : $"{Title.ToLower()} record(s) found");
            Created = Culture == Cultures.TR ? $"{Title} başarıyla oluşturuldu" : $"{Title} created successfully";
            Updated = Culture == Cultures.TR ? $"{Title} başarıyla güncellendi" : $"{Title} updated successfully";
            Deleted = Culture == Cultures.TR ? $"{Title} başarıyla silindi" : $"{Title} deleted successfully";
        }

        public Result Success(int? id = default)
        {
            return new Result(HttpStatusCode.OK, null, id);
        }

        public Result Success(string message, int? id = default)
        {
            if (!message.EndsWith("."))
                message += ".";
            return new Result(HttpStatusCode.OK, message, id);
        }

        public Result Error(HttpStatusCode httpStatusCode)
        {
            return new Result(httpStatusCode,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound :
                httpStatusCode == HttpStatusCode.Unauthorized ? Unauthorized : Failed);
        }

        public Result Error(string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed};{message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result(httpStatusCode,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound :
                httpStatusCode == HttpStatusCode.Unauthorized ? Unauthorized : message);
        }

        public Result<TData> Success<TData>(TData data) where TData : class, new()
        {
            return new Result<TData>(HttpStatusCode.OK, data);
        }

        public Result<TData> Success<TData>(TData data, string message, HttpStatusCode httpStatusCode = HttpStatusCode.OK) where TData : class, new()
        {
            if (!message.EndsWith("."))
                message += ".";
            return new Result<TData>(httpStatusCode, data, message);
        }

        public Result<TData> Success<TData>(TData data, Result result) where TData : class, new()
        {
            var message = result.Message;
            if (!string.IsNullOrWhiteSpace(message) && !message.EndsWith("."))
                message += ".";
            return new Result<TData>(result.HttpStatusCode, data, message);
        }

        public Result<TData> Error<TData>(TData data) where TData : class, new()
        {
            return new Result<TData>(HttpStatusCode.BadRequest, data, Failed);
        }

        public Result<TData> Error<TData>(TData data, HttpStatusCode httpStatusCode) where TData : class, new()
        {
            return new Result<TData>(httpStatusCode, data,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound :
                httpStatusCode == HttpStatusCode.Unauthorized ? Unauthorized : Failed);
        }

        public Result<TData> Error<TData>(TData data, string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) where TData : class, new()
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed};{message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(httpStatusCode, data,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : 
                httpStatusCode == HttpStatusCode.Unauthorized ? Unauthorized : message);
        }

        public Result<TData> Error<TData>(TData data, Result result) where TData : class, new()
        {
            var message = result.Message;
            if (!message.StartsWith(Failed))
                message = $"{Failed};{message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(result.HttpStatusCode, data,
                result.HttpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                result.HttpStatusCode == HttpStatusCode.NotFound ? NotFound :
                result.HttpStatusCode == HttpStatusCode.Unauthorized ? Unauthorized : message);
        }

        public Result Validate(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors(Culture);
            if (errors.Any())
                return Error(string.Join(";", errors));
            return Success();
        }

        public void LogError(string message)
        {
            LogService.Error(message);
        }
    }
}
