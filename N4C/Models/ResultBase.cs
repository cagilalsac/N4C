using System.Net;

namespace N4C.Models
{
    public class Result
    {
        public HttpStatusCode HttpStatusCode { get; }
        public string Message { get; }
        public int? Id { get; }
        public string Culture { get; }
        public string Title { get; }
        public bool Success => (int)HttpStatusCode >= 200 && (int)HttpStatusCode <= 299;

        public Result(HttpStatusCode httpStatusCode, string message = default, string culture = default, string title = default, int? id = default)
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
            Culture = culture ?? Cultures.TR;
            Title = title is null ? (Culture == Cultures.TR ? "Kayıt" : "Record") : title;
            Id = id;
        }
    }

    public interface IResult<TData> where TData : class, new()
    {
        public TData Data { get; set; }
    }
}
