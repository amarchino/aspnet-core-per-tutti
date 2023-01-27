using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Customizations.TagHelpers
{
    [HtmlTargetElement("price")]
    public class PriceTagHelper : TagHelper
    {
        public Money CurrentPrice { get; set; }
        public Money FullPrice { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Content.AppendHtml($"{CurrentPrice}");
            if(!CurrentPrice.Equals(FullPrice))
            {
                output.Content.AppendHtml($"<br/><s>{FullPrice}</s>");
            }
        }
    }
}
