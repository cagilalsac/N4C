using System.Net;

namespace N4C.App
{
    public class View
    {
        public string Culture { get; }
        public string Message { get; }
        public string Title { get; }
        public PageOrder PageOrder { get; }
        public HttpStatusCode HttpStatusCode { get; }

        public View(string culture, string title = default, string message = default, HttpStatusCode httpStatusCode = HttpStatusCode.OK, PageOrder pageOrder = default)
        {
            Culture = culture;
            Title = string.IsNullOrWhiteSpace(title) ? string.Empty : title;
            Message = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            HttpStatusCode = httpStatusCode;
            PageOrder = pageOrder is not null && pageOrder.PageNumber != 0 ? pageOrder : null;
        }
    }
}
