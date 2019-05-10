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
using Umbraco.Web.Composing;

namespace Our.Umbraco.InnerContent.Helpers
{
    public static class InnerContentHelper
    {
        public static IEnumerable<IPublishedElement> ConvertInnerContentToPublishedElement(JArray items, bool preview = false)
        {
            return items.Select(x => ConvertInnerContentToPublishedElement((JObject)x, preview)).ToList();
        }

        public static IPublishedElement ConvertInnerContentToPublishedElement(JObject item, bool preview = false)
        {
            // TODO: Implement this properly. I've no idea what the `owner` is meant to be. [LK:2019-04-02]
            var owner = default(IPublishedElement);

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
                    properties.Add(new DetachedPublishedProperty(propType, owner, jProp.Value, preview));
                }
            }

            // Manually parse out the special properties
            propValues.TryGetValue("name", out object nameObj);
            propValues.TryGetValue("key", out object keyObj);

            var node = new DetachedPublishedElement(
                keyObj == null ? Guid.Empty : Guid.Parse(keyObj.ToString()),
                nameObj?.ToString(),
                publishedContentType,
                properties.ToArray());

            // TODO: Implement later [LK:2019-04-02] (I'm not sure how we're going to deal with IPublishedElement not having "children"
            //// Process children
            //if (propValues.ContainsKey("children"))
            //{
            //    var children = ConvertInnerContentToPublishedContent((JArray)propValues["children"], node, level + 1, preview);
            //    node.SetChildren(children);
            //}

            // TODO: Implement later [LK:2019-04-02]
            var factory = Current.Factory.GetInstance<IPublishedModelFactory>();
            if (factory != null)
            {
                // Let the current model factory create a typed model to wrap our model
                return factory.CreateModel(node);
            }

            return node;
        }

        // TODO: Implement later [LK:2019-04-02]
        //internal static PreValueCollection GetPreValuesCollectionByDataTypeId(int dtdId)
        //{
        //    var preValueCollection = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<PreValueCollection>(
        //        string.Format(InnerContentConstants.PreValuesCacheKey, dtdId),
        //        () => ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dtdId));

        //    return preValueCollection;
        //}

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
            var contentTypeService = Current.Services.ContentTypeService;

            var contentTypeGuid = GetContentTypeGuidFromItem(item);
            if (contentTypeGuid.HasValue && contentTypeGuid.Value != Guid.Empty)
                return contentTypeService.Get(contentTypeGuid.Value);

            var contentTypeAlias = GetContentTypeAliasFromItem(item);
            if (string.IsNullOrWhiteSpace(contentTypeAlias) == false)
            {
                // Future-proofing - setting the GUID, queried from the alias
                SetContentTypeGuid(item, contentTypeAlias, contentTypeService);

                return contentTypeService.Get(contentTypeAlias);
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

        internal static IPublishedContentType GetPublishedContentTypeFromItem(JObject item)
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

                ContentTypeCacheHelper.TryGetAlias(contentTypeGuid.Value, out contentTypeAlias, Current.Services.ContentTypeService);
            }

            // If we don't have the content-type alias at this point, check if we can get it from the item
            if (string.IsNullOrEmpty(contentTypeAlias))
                contentTypeAlias = GetContentTypeAliasFromItem(item);

            if (string.IsNullOrEmpty(contentTypeAlias))
                return null;

            // TODO: Implement properly later [LK:2019-04-02]
            return Current.PublishedSnapshot.Content.GetContentType(contentTypeAlias);
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

                    var prop = new Property(propType);
                    prop.SetValue(jProp.Value); // TODO: Check if this is the correct way to do this? Confused about the culture/segment bits. [LK:2019-04-02]

                    properties.Add(prop);
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