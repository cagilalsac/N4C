using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;

namespace N4C.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent DisplayNameFor<TModel, TProperty>(this IHtmlHelper<TModel> helper,
                Expression<Func<TModel, TProperty>> expression, string culture)
        {
            ModelExpressionProvider modelExpressionProvider = (ModelExpressionProvider)helper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(ModelExpressionProvider));
            ModelExpression modelExpression = modelExpressionProvider.CreateModelExpression(helper.ViewData, expression);
            string propertyName = modelExpression.Metadata.Name;
            if (!string.IsNullOrWhiteSpace(modelExpression.Metadata.PropertyName))
                propertyName = modelExpression.Metadata.PropertyName;
            string displayName = modelExpression.Metadata.DisplayName.GetDisplayName(propertyName, culture);
            TagBuilder labelTag = new TagBuilder("label");
            labelTag.Attributes.Add("for", helper.IdFor(expression).ToString());
            labelTag.Attributes.Add("style", "cursor:pointer");
            labelTag.InnerHtml.AppendHtml(displayName);
            return labelTag;
        }

        public static IHtmlContent ValidationMessageFor<TModel, TProperty>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression, string culture)
        {
            string propertyName = expression.Body.ToString();
            if (propertyName.Contains("."))
                propertyName = propertyName.Remove(0, propertyName.IndexOf(".") + 1);
            var modelState = helper.ViewContext.ModelState;
            TagBuilder spanTag = new TagBuilder("span");
            if (modelState.TryGetValue(propertyName, out var entry) && entry.Errors.Count > 0)
            {
                var errorMessage = entry.Errors[0].ErrorMessage;
                errorMessage = errorMessage.GetErrorMessage(propertyName, culture);
                spanTag.Attributes.Add("class", "text-danger");
                spanTag.InnerHtml.AppendHtml(errorMessage);
            }
            return spanTag;
        }
    }
}
