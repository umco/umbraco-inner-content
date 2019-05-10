using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.InnerContent.Helpers
{
    internal static class ContentTypeCacheHelper
    {
        private static readonly ConcurrentDictionary<Guid, string> Forward = new ConcurrentDictionary<Guid, string>();
        private static readonly ConcurrentDictionary<string, Guid> Reverse = new ConcurrentDictionary<string, Guid>();

        public static void ClearAll()
        {
            Forward.Clear();
            Reverse.Clear();
        }

        public static void TryAdd(IContentType contentType)
        {
            TryAdd(contentType.Key, contentType.Alias);
        }

        public static void TryAdd(Guid guid, string alias)
        {
            Forward.TryAdd(guid, alias);
            Reverse.TryAdd(alias, guid);
        }

        public static bool TryGetAlias(Guid key, out string alias, IContentTypeService contentTypeService = null)
        {
            if (Forward.TryGetValue(key, out alias))
                return true;

            // The alias isn't cached, we can attempt to get it via the content-type service, using the GUID.
            if (contentTypeService != null)
            {
                var contentType = contentTypeService.Get(key);
                if (contentType != null)
                {
                    TryAdd(contentType);
                    alias = contentType.Alias;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetGuid(string alias, out Guid key, IContentTypeService contentTypeService = null)
        {
            if (Reverse.TryGetValue(alias, out key))
                return true;

            // The GUID isn't cached, we can attempt to get it via the content-type service, using the alias.
            if (contentTypeService != null)
            {
                var contentType = contentTypeService.Get(alias);
                if (contentType != null)
                {
                    TryAdd(contentType);
                    key = contentType.Key;
                    return true;
                }
            }

            return false;
        }

        public static void TryRemove(IContentType contentType)
        {
            if (TryRemove(contentType.Alias) == false)
            {
                TryRemove(contentType.Key);
            }
        }

        public static bool TryRemove(Guid guid)
        {
            return Forward.TryRemove(guid, out string alias)
                ? Reverse.TryRemove(alias, out guid)
                : false;
        }

        public static bool TryRemove(string alias)
        {
            return Reverse.TryRemove(alias, out Guid guid)
                ? Forward.TryRemove(guid, out alias)
                : false;
        }
    }
}