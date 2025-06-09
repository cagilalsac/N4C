using System.Net;

namespace N4C.Models
{
    public class Result<TData> : Result where TData : class, new()
    {
        public TData Data { get; set; }

        public Result(HttpStatusCode httpStatusCode, TData data, Page page, Order order, string message = default, string culture = default, string title = default,
            bool modelStateErrors = true) : base(httpStatusCode, page, order, message, culture, title, modelStateErrors)
        {
            Data = data;
        }
    }
}
