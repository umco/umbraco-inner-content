using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
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

        public static IPublishedContent ConvertInnerContentToPublishedContent(
            JObject item,
            IPublishedContent parentNode = null,
            int sortOrder = 0,
            int level = 0,
            bool preview = false)
        {
            var publishedContentType = GetPublishedContentTypeFromItem(item);
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
            var preValueCollection = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<PreValueCollection>(
                string.Format(InnerContentConstants.PreValuesCacheKey, dtdId),
                () => ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dtdId));

            return preValueCollection;
        }

        internal static string GetContentTypeAliasFromItem(JObject item)
        {
            var contentTypeAliasProperty = item?[InnerContentConstants.ContentTypeAliasPropertyKey];
            return contentTypeAliasProperty?.ToObject<string>();
        }

        internal static Guid? GetContentTypeGuidFromItem(JObject item)
        {
            var contentTypeGuidProperty = item?[InnerContentConstants.ContentTypeGuidPropertyKey];
            return contentTypeGuidProperty?.ToObject<Guid?>();
        }

        internal static IContentType GetContentTypeFromItem(JObject item)
        {
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

            var contentTypeGuid = GetContentTypeGuidFromItem(item);
            if (contentTypeGuid.HasValue && contentTypeGuid.Value != Guid.Empty)
                return contentTypeService.GetContentType(contentTypeGuid.Value);

            var contentTypeAlias = GetContentTypeAliasFromItem(item);
            if (string.IsNullOrWhiteSpace(contentTypeAlias) == false)
            {
                // Future-proofing - setting the GUID, queried from the alias
                SetContentTypeGuid(item, contentTypeAlias, contentTypeService);

                return contentTypeService.GetContentType(contentTypeAlias);
            }

            return null;
        }

        internal static void SetContentTypeGuid(JObject item, string contentTypeAlias, IContentTypeService contentTypeService)
        {
            if (ContentTypeCacheHelper.TryGetGuid(contentTypeAlias, out Guid key, contentTypeService))
            {
                item[InnerContentConstants.ContentTypeGuidPropertyKey] = key.ToString();
            }
        }

        internal static PublishedContentType GetPublishedContentTypeFromItem(JObject item)
        {
            var contentTypeAlias = string.Empty;

            // First we check if the item has a content-type GUID...
            var contentTypeGuid = GetContentTypeGuidFromItem(item);
            if (contentTypeGuid.HasValue)
            {
                // HACK: If Umbraco's `PublishedContentType.Get` method supported a GUID parameter,
                // we could use that method directly, however it only supports the content-type alias (as of v7.4.0)
                // See: https://github.com/umbraco/Umbraco-CMS/blob/release-7.4.0/src/Umbraco.Core/Models/PublishedContent/PublishedContentType.cs#L133
                // Our workaround is to cache a content-type GUID => alias lookup.

                ContentTypeCacheHelper.TryGetAlias(contentTypeGuid.Value, out contentTypeAlias, ApplicationContext.Current.Services.ContentTypeService);
            }

            // If we don't have the content-type alias at this point, check if we can get it from the item
            if (string.IsNullOrEmpty(contentTypeAlias))
                contentTypeAlias = GetContentTypeAliasFromItem(item);

            if (string.IsNullOrEmpty(contentTypeAlias))
                return null;

            return PublishedContentType.Get(PublishedItemType.Content, contentTypeAlias);
        }

        internal static IContent ConvertInnerContentToBlueprint(JObject item, int userId = 0)
        {
            var contentType = GetContentTypeFromItem(item);

            // creates a fast lookup of the property types
            var propertyTypes = contentType.PropertyTypes.ToDictionary(x => x.Alias, x => x, StringComparer.InvariantCultureIgnoreCase);

            var propValues = item.ToObject<Dictionary<string, object>>();
            var properties = new List<Property>();

            foreach (var jProp in propValues)
            {
                if (propertyTypes.ContainsKey(jProp.Key) == false)
                    continue;

                var propType = propertyTypes[jProp.Key];
                if (propType != null)
                {
                    // TODO: Check if we need to call `ConvertEditorToDb`?
                    properties.Add(new Property(propType, jProp.Value));
                }
            }

            // Manually parse out the special properties
            propValues.TryGetValue("name", out object name);
            propValues.TryGetValue("key", out object key);

            return new Content(name?.ToString(), -1, contentType, new PropertyCollection(properties))
            {
                Key = key == null ? Guid.Empty : Guid.Parse(key.ToString()),
                ParentId = -1,
                Path = "-1",
                CreatorId = userId,
                WriterId = userId
            };
        }
    }
}