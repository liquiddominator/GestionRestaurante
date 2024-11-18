using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace restaurante
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent GenerateInventarioOptions(this IHtmlHelper htmlHelper, IEnumerable<SelectListItem> items, string selectedValue)
        {
            var builder = new HtmlContentBuilder();
            foreach (var item in items)
            {
                TagBuilder option = new TagBuilder("option");
                option.Attributes["value"] = item.Value;
                if (item.Value == selectedValue)
                {
                    option.Attributes["selected"] = "selected";
                }
                option.InnerHtml.Append(item.Text);
                builder.AppendHtml(option);
            }
            return builder;
        }
    }
}
