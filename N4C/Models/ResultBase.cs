using System.Net;

namespace N4C.Models
{
    public class Result
    {
        public HttpStatusCode HttpStatusCode { get; }
        public string Message { get; }
        public int? Id { get; }
        public bool ModelStateErrors { get; }
        public string Culture { get; }
        public string Title { get; }
        public bool Success => (int)HttpStatusCode >= 200 && (int)HttpStatusCode <= 299;

        public Page Page { get; }
        public Order Order { get; }

        public Result(HttpStatusCode httpStatusCode, string message = default, string culture = default, string title = default, 
            int? id = default, bool modelStateErrors = true)
        {
            HttpStatusCode = httpStatusCode;
            Message = message ?? string.Empty;
            if (Message != string.Empty)
            {
                if (Success && !message.EndsWith("."))
                    Message += ".";
                else if (!Success && !message.EndsWith("!"))
                    Message += "!";
            }
            Culture = culture ?? Defaults.TR;
            Title = title is null ? (Culture == Defaults.TR ? "Kayıt" : "Record") : title;
            Id = id;
            ModelStateErrors = modelStateErrors;
        }

        public Result(HttpStatusCode httpStatusCode, Page page, Order order, string message = default, string culture = default, string title = default,
            bool modelStateErrors = true) : this(httpStatusCode, message, culture, title, null, modelStateErrors)
        {
            Page = page;
            Order = order;
        }
    }
}
