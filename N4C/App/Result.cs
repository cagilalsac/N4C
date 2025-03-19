using System.Net;

namespace N4C.App
{
    public interface IResult
    {
        public HttpStatusCode HttpStatusCode { get; }
        public string Message { get; }
        public bool Success { get; }
        public string Type { get; }
    }

    public interface IResult<out TData>
    {
        public TData Data { get; }
    }

    public class Result : IResult
    {
        public HttpStatusCode HttpStatusCode { get; }
        public string Message { get; }
        public bool Success => ((int)HttpStatusCode).ToString().StartsWith("2");
        public string Type => typeof(Result).ToString();

        public Result(HttpStatusCode httpStatusCode, string message = default)
        {
            HttpStatusCode = httpStatusCode;
            Message = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
        }
    }

    public class Result<TData> : Result, IResult<TData>
    {
        public TData Data { get; }

        public Result(HttpStatusCode httpStatusCode, TData data = default, string message = default) : base(httpStatusCode, message)
        {
            Data = data;
        }
    }
}
