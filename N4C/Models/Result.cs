using System.Net;

namespace N4C.Models
{
    public class Result<TData> : Result where TData : class, new()
    {
        public TData Data { get; set; }

        public Result(HttpStatusCode httpStatusCode, TData data = default, string message = default, string culture = default, string title = default, int? id = default) 
            : base(httpStatusCode, message, culture, title, id)
        {
            Data = data;
        }

        public Result(HttpStatusCode httpStatusCode, TData data, Page page, Order order, string message = default, string culture = default, string title = default)
            : base(httpStatusCode, page, order, message, culture, title)
        {
            Data = data;
        }
    }
}
