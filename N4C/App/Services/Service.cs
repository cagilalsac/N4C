using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using N4C.Extensions;
using System.Globalization;
using System.Net;

namespace N4C.App.Services
{
    public class Service
    {
        public string Culture { get; private set; }
        public string Title { get; private set; }

        protected string Failed { get; private set; }
        protected string Successful { get; private set; }
        protected string Exception { get; private set; }

        protected string NotFound { get; set; }

        protected HttpService HttpService { get; }
        protected ILogger<Service> Logger { get; }

        public Service(HttpService httpService, ILogger<Service> logger)
        {
            HttpService = httpService;
            Logger = logger;
            SetCulture(HttpService.GetCookie("Culture") ?? Settings.Culture);
        }

        protected virtual void SetCulture(string culture)
        {
            Culture = culture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
            Failed = Culture == Cultures.TR ? "İşlem gerçekleştirilemedi!" : "Operation failed!";
            Successful = Culture == Cultures.TR ? "İşlem başarıyla gerçekleştirildi." : "Operation successful.";
            Exception = Culture == Cultures.TR ? "İşlem sırasında hata meydana geldi!" : "An exception occurred during the operation!";
            NotFound = Culture == Cultures.TR ? "Kayıt bulunamadı!" : "Record not found!";
        }

        protected virtual void SetTitle(string tr, string en = default)
        {
            Title = !string.IsNullOrWhiteSpace(en) && Culture == Cultures.EN ? en : tr;
            NotFound = Culture == Cultures.TR ? $"{Title} bulunamadı" : $"{Title} not found";
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

        public Result Error(string message)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed} {message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result(HttpStatusCode.BadRequest, message);
        }

        protected Result<TData> Success<TData>(TData data)
        {
            return new Result<TData>(HttpStatusCode.OK, data);
        }

        protected Result<TData> Success<TData>(TData data, string message)
        {
            if (!message.EndsWith("."))
                message += ".";
            return new Result<TData>(HttpStatusCode.OK, data, message);
        }

        protected Result<TData> Success<TData>(TData data, Result result)
        {
            var message = result.Message;
            if (!string.IsNullOrWhiteSpace(message) && !message.EndsWith("."))
                message += ".";
            return new Result<TData>(HttpStatusCode.OK, data, message);
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

        protected Result<TData> Error<TData>(TData data, string message)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed} {message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(HttpStatusCode.BadRequest, data, message);
        }

        protected Result<TData> Error<TData>(TData data, Result result)
        {
            var message = result.Message;
            if (!message.StartsWith(Failed))
                message = $"{Failed} {message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(result.HttpStatusCode, data,
                result.HttpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                result.HttpStatusCode == HttpStatusCode.NotFound ? NotFound : message);
        }

        public Result<List<string>> Validate(ModelStateDictionary modelState, string errorMessagesSeperator = "; ")
        {
            List<string> errorMessages = new List<string>();
            if (modelState is not null && modelState.Any())
            {
                errorMessages.AddRange(modelState.GetErrors(Culture));
                if (errorMessages.Any())
                {
                    if (errorMessagesSeperator == "<br>" || errorMessagesSeperator == "<br />" || errorMessagesSeperator == "<br/>")
                        errorMessages.Insert(0, string.Empty);
                    return Error(errorMessages, string.Join(errorMessagesSeperator, errorMessages));
                }
            }
            return Success(errorMessages);
        }
    }
}
