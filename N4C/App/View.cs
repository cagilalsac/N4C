using System.Net;

namespace N4C.App
{
    public class View
    {
        public string Culture { get; }
        public string Message { get; }
        public string Title { get; } = string.Empty;
        public PageOrder PageOrder { get; }
        public HttpStatusCode HttpStatusCode { get; }

        public View(string culture, string message = default, HttpStatusCode httpStatusCode = HttpStatusCode.OK, string title = default, PageOrder pageOrder = default)
        {
            Culture = culture;
            Message = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            HttpStatusCode = httpStatusCode;
            Title = string.IsNullOrWhiteSpace(title) ? string.Empty : title;
            PageOrder = pageOrder is not null && pageOrder.PageNumber != 0 ? pageOrder : null;
        }
    }
}
