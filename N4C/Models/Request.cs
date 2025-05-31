using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class Request : Data
    {
        [JsonIgnore]
        public ModelStateDictionary ModelState { get; private set; } = new ModelStateDictionary();

        public void Set(ModelStateDictionary modelState)
        {
            ModelState = modelState;
        }

        public void Add(string tr, string en = default)
        {
            ModelState.AddModelError(string.Empty, tr + ";" + en);
        }
    }
}
