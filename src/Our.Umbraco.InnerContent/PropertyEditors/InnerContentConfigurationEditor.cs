using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core.Composing;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class InnerContentConfigurationEditor : ConfigurationEditor
    {
        protected bool TryEnsureContentTypeGuids(JArray items)
        {
            if (items == null)
                return false;

            var ensured = false;

            foreach (JObject item in items)
            {
                var contentTypeGuid = item[InnerContentConstants.ContentTypeGuidPropertyKey];
                if (contentTypeGuid != null)
                    continue;

                var contentTypeAlias = item[InnerContentConstants.ContentTypeAliasPropertyKey];
                if (contentTypeAlias == null)
                    continue;

                InnerContentHelper.SetContentTypeGuid(item, contentTypeAlias.Value<string>(), Current.Services.ContentTypeService);
                ensured = true;
            }

            return ensured;
        }
    }
}