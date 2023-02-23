using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MyCourse.Customizations.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "asp-for")]
    public class InputNumberTagHelper : TagHelper
    {
        public override int Order => int.MaxValue;

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            bool isNumberType = output.Attributes.Any(attribute => "type".Equals(attribute.Name, StringComparison.InvariantCultureIgnoreCase)
                && "number".Equals(attribute.Value as string, StringComparison.InvariantCultureIgnoreCase));
            if(!isNumberType)
            {
                return;
            }
            if(For.ModelExplorer.ModelType != typeof(decimal))
            {
                return;
            }
            if(For.Model == null)
            {
                return;
            }

            decimal value = (decimal)For.Model;
            string formattedValue = value.ToString("F2", CultureInfo.InvariantCulture);
            output.Attributes.SetAttribute("value", formattedValue);
        }
    }
}
