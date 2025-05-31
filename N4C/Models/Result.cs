using System.Net;

namespace N4C.Models
{
    public class Result<TData> : Result, IResult<TData> where TData : class, new()
    {
        public TData Data { get; set; }

        public Result(HttpStatusCode httpStatusCode, TData data = default, string message = default, string culture = default, string title = default, int? id = default) 
            : base(httpStatusCode, message, culture, title, id)
        {
            Data = data;
        }
    }
}
