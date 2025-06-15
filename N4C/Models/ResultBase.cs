using System.Net;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class Result
    {
        public HttpStatusCode HttpStatusCode { get; }
        public string Message { get; }
        public int? Id { get; }

        [JsonIgnore]
        public bool ModelStateErrors { get; }

        [JsonIgnore]
        public string Culture { get; }

        [JsonIgnore]
        public string Title { get; }

        [JsonIgnore]
        public bool Success => (int)HttpStatusCode >= 200 && (int)HttpStatusCode <= 299;

        [JsonIgnore]
        public Page Page { get; }

        [JsonIgnore]
        public Order Order { get; }

        public string Type => typeof(Result).ToString();

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
