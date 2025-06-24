using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace N4C.Extensions.TagHelpers
{
    [HtmlTargetElement("displayname", Attributes = "asp-for,asp-culture")]
    public class DisplayNameTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression AspFor { get; set; }

        [HtmlAttributeName("asp-culture")]
        public string AspCulture { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ModelMetadata aspForMetadata = AspFor.Metadata;
            string propertyName = aspForMetadata.Name;
            if (aspForMetadata.PropertyName.HasAny())
                propertyName = aspForMetadata.PropertyName;
            string displayName = aspForMetadata.DisplayName.GetDisplayName(propertyName, AspCulture);
            output.TagName = "label";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetContent(displayName);
        }
    }
}
