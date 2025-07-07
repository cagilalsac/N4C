using Microsoft.AspNetCore.Http;
using N4C.Extensions;
using System.Text.Json;

namespace N4C.Models
{
    public class ApiRequest<TRequest> where TRequest : class, new()
    {
        public TRequest Request => RequestJson.HasAny() ? JsonSerializer.Deserialize<TRequest>(RequestJson) : null;
        public string RequestJson { get; set; }
        public IFormFile MainFormFile { get; set; }
        public List<IFormFile> OtherFormFiles { get; set; }
    }
}
