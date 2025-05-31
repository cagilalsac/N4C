using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace N4C.Extensions.TagHelpers
{
    [HtmlTargetElement("validation", Attributes = "asp-for,asp-culture")]
    public class ValidationTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression AspFor { get; set; }

        [HtmlAttributeName("asp-culture")]
        public string AspCulture { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = AspFor.Name.Contains('.') ? AspFor.Name.Split('.')[AspFor.Name.Split('.').Length - 1] : AspFor.Name;
            var modelState = ViewContext.ViewData.ModelState;
            if (modelState.TryGetValue(propertyName, out var entry) && entry.Errors.Count > 0)
            {
                var errorMessage = entry.Errors[0].ErrorMessage;
                errorMessage = errorMessage.GetErrorMessage(propertyName, AspCulture);
                output.TagName = "span";
                output.Content.SetHtmlContent(errorMessage);
            }
            else
            {
                output.SuppressOutput();
            }
        }
    }
}
