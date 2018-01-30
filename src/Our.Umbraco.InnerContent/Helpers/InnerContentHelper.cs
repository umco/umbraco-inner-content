using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Our.Umbraco.InnerContent.Helpers
{
    public static class InnerContentHelper
    {
        public static IEnumerable<IPublishedContent> ConvertInnerContentToPublishedContent(
            JArray items,
            IPublishedContent parentNode = null,
            int level = 0,
            bool preview = false)
        {
            return items.Select((x, i) => ConvertInnerContentToPublishedContent((JObject)x, parentNode, i, level, preview)).ToList();
        }

        public static IPublishedContent ConvertInnerContentToPublishedContent(JObject item,
            IPublishedContent parentNode = null,
            int sortOrder = 0,
            int level = 0,
            bool preview = false)
        {
            var contentTypeAlias = GetContentTypeAliasFromItem(item);
            if (string.IsNullOrEmpty(contentTypeAlias))
                return null;

            var publishedContentType = PublishedContentType.Get(PublishedItemType.Content, contentTypeAlias);
            if (publishedContentType == null)
                return null;

            var propValues = item.ToObject<Dictionary<string, object>>();
            var properties = new List<IPublishedProperty>();

            foreach (var jProp in propValues)
            {
                var propType = publishedContentType.GetPropertyType(jProp.Key);
                if (propType != null)
                {
                    properties.Add(new DetachedPublishedProperty(propType, jProp.Value, preview));
                }
            }

            // Manually parse out the special properties
            propValues.TryGetValue("name", out object nameObj);
            propValues.TryGetValue("key", out object keyObj);

            // Get the current request node we are embedded in
            var pcr = UmbracoContext.Current.PublishedContentRequest;
            var containerNode = pcr != null && pcr.HasPublishedContent ? pcr.PublishedContent : null;

            var node = new DetachedPublishedContent(
                keyObj == null ? Guid.Empty : Guid.Parse(keyObj.ToString()),
                nameObj?.ToString(),
                publishedContentType,
                properties.ToArray(),
                containerNode,
                parentNode,
                sortOrder,
                level,
                preview);

            // Process children
            if (propValues.ContainsKey("children"))
            {
                var children = ConvertInnerContentToPublishedContent((JArray)propValues["children"], node, level + 1, preview);
                node.SetChildren(children);
            }

            if (PublishedContentModelFactoryResolver.HasCurrent && PublishedContentModelFactoryResolver.Current.HasValue)
            {
                // Let the current model factory create a typed model to wrap our model
                return PublishedContentModelFactoryResolver.Current.Factory.CreateModel(node);
            }

            return node;
        }

        internal static PreValueCollection GetPreValuesCollectionByDataTypeId(int dtdId)
        {
            var preValueCollection = (PreValueCollection)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                string.Format(InnerContentConstants.PreValuesCacheKey, dtdId),
                () => ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dtdId));

            return preValueCollection;
        }

        internal static string GetContentTypeAliasFromItem(JObject item)
        {
            var contentTypeAliasProperty = item?[InnerContentConstants.ContentTypeAliasPropertyKey];
            return contentTypeAliasProperty?.ToObject<string>();
        }

        internal static IContentType GetContentTypeFromItem(JObject item)
        {
            var contentTypeAlias = GetContentTypeAliasFromItem(item);
            return !contentTypeAlias.IsNullOrWhiteSpace()
                ? ApplicationContext.Current.Services.ContentTypeService.GetContentType(contentTypeAlias)
                : null;
        }
    }
}