using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Extensions;
using System.Globalization;
using System.Net;

namespace N4C.App.Services
{
    public abstract class Service
    {
        public string Culture { get; private set; } = Settings.Culture;

        protected string Failed { get; private set; } = "Operation failed!";
        protected string Successful { get; private set; } = "Operation successful.";
        protected string Exception { get; private set; } = "An exception occurred during the operation!";

        protected string NotFound { get; set; } = "Record not found!";
        protected string Found { get; set; } = "record(s) found.";
        protected string Created { get; set; } = "Record created successfully.";
        protected string Updated { get; set; } = "Record updated successfully.";
        protected string Deleted { get; set; } = "Record deleted successfully.";

        protected LogService LogService { get; }

        protected Service(LogService logService)
        {
            LogService = logService;
        }

        internal virtual void SetCulture(string culture)
        {
            Culture = culture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
            Failed = Culture == Cultures.TR ? "İşlem gerçekleştirilemedi!" : "Operation failed!";
            Successful = Culture == Cultures.TR ? "İşlem başarıyla gerçekleştirildi." : "Operation successful.";
            Exception = Culture == Cultures.TR ? "İşlem sırasında hata meydana geldi!" : "An exception occurred during the operation!";
            NotFound = Culture == Cultures.TR ? "Kayıt bulunamadı!" : "Record not found!";
            Found = Culture == Cultures.TR ? "bulundu." : "record(s) found.";
            Created = Culture == Cultures.TR ? "Kayıt başarıyla oluşturuldu." : "Record created successfully.";
            Updated = Culture == Cultures.TR ? "Kayıt başarıyla güncellendi." : "Record updated successfully.";
            Deleted = Culture == Cultures.TR ? "Kayıt başarıyla silindi." : "Record deleted successfully.";
        }

        public Result Success()
        {
            return new Result(HttpStatusCode.OK);
        }

        public Result Success(string message)
        {
            if (!message.EndsWith("."))
                message += ".";
            return new Result(HttpStatusCode.OK, message);
        }

        public Result Error(HttpStatusCode httpStatusCode)
        {
            return new Result(httpStatusCode,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : Failed);
        }

        public Result Error(string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed};{message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result(httpStatusCode,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : message);
        }

        protected Result<TData> Success<TData>(TData data)
        {
            return new Result<TData>(HttpStatusCode.OK, data);
        }

        protected Result<TData> Success<TData>(TData data, string message, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            if (!message.EndsWith("."))
                message += ".";
            return new Result<TData>(httpStatusCode, data, message);
        }

        protected Result<TData> Success<TData>(TData data, Result result)
        {
            var message = result.Message;
            if (!string.IsNullOrWhiteSpace(message) && !message.EndsWith("."))
                message += ".";
            return new Result<TData>(result.HttpStatusCode, data, message);
        }

        protected Result<TData> Error<TData>(TData data)
        {
            return new Result<TData>(HttpStatusCode.BadRequest, data, Failed);
        }

        protected Result<TData> Error<TData>(TData data, HttpStatusCode httpStatusCode)
        {
            return new Result<TData>(httpStatusCode, data,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : Failed);
        }

        protected Result<TData> Error<TData>(TData data, string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed};{message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(httpStatusCode, data,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : message);
        }

        protected Result<TData> Error<TData>(TData data, Result result)
        {
            var message = result.Message;
            if (!message.StartsWith(Failed))
                message = $"{Failed};{message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(result.HttpStatusCode, data,
                result.HttpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                result.HttpStatusCode == HttpStatusCode.NotFound ? NotFound : message);
        }

        protected Result Validate(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors(Culture);
            if (errors.Any())
                return Error(string.Join(";", errors));
            return Success();
        }
    }
}
