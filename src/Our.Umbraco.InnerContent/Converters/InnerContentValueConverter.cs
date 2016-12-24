using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.Converters
{
    public abstract class InnerContentValueConverter : PropertyValueConverterBase
    {
        protected IEnumerable<IPublishedContent> ConvertInnerContentDataToSource(
            JArray items,
            IPublishedContent parentNode = null,
            int level = 0,
            bool preview = false)
        {
            return InnerContentHelper.ConvertInnerContentToPublishedContent(items, parentNode, level, preview);
        }

        protected IPublishedContent ConvertInnerContentDataToSource(
            JObject item,
            IPublishedContent parentNode = null,
            int sortOrder = 0,
            int level = 0,
            bool preview = false)
        {
            return InnerContentHelper.ConvertInnerContentToPublishedContent(item, parentNode, sortOrder, level, preview);
        }
    }
}
