using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace N4C.Extensions
{
    public static class ModelStateExtensions
    {
        public static List<string> GetErrors(this ModelStateDictionary modelState, string culture)
        {
            List<string> errorMessages = new List<string>();
            if (modelState is not null)
            {
                var modelStateErrors = modelState.Where(ms => ms.Value.Errors.Any()).Select(ms => new { ms.Key, ms.Value.Errors }).ToList();
                foreach (var modelStateError in modelStateErrors)
                {
                    errorMessages.AddRange(modelStateError.Errors.Select(e => e.ErrorMessage.GetErrorMessage(modelStateError.Key, culture)));
                }
            }
            return errorMessages.Order().ToList();
        }
    }
}
