namespace N4C.App.Services.Models
{
    public abstract class ViewModel<TRequest, TResponse> where TRequest : Request, new() where TResponse : Response, new()
    {
        public List<TResponse> Data { get; set; }
        public TRequest Request { get; set; }
        public int MvcAction { get; set; }
    }
}
