using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.ValueConverters
{
    public abstract class InnerContentValueConverter : PropertyValueConverterBase
    {
        protected IEnumerable<IPublishedElement> ConvertInnerContentDataToSource(
            JArray items,
            bool preview = false)
        {
            return InnerContentHelper.ConvertInnerContentToPublishedElement(items, preview);
        }

        protected IPublishedElement ConvertInnerContentDataToSource(
            JObject item,
            bool preview = false)
        {
            return InnerContentHelper.ConvertInnerContentToPublishedElement(item, preview);
        }
    }
}