using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace N4C.Extensions.TagHelpers
{
    [HtmlTargetElement("inputarea", Attributes = "asp-for")]
    public class InputAreaTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression AspFor { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ModelMetadata aspForMetadata = AspFor.Metadata;
            string propertyName = aspForMetadata.Name;
            if (!string.IsNullOrWhiteSpace(aspForMetadata.PropertyName))
                propertyName = aspForMetadata.PropertyName;
            output.TagName = "textarea";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add(new TagHelperAttribute("name", propertyName));
            output.Attributes.Add(new TagHelperAttribute("rows", 3));
        }
    }
}
