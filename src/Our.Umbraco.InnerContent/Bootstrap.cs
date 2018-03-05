using System.Linq;
using Newtonsoft.Json;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Our.Umbraco.InnerContent
{
    public class Bootstrap : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DataTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType != MessageType.RefreshByJson)
                    return;

                // NOTE: The properties for the JSON payload are available here: (Currently there isn't a public API to deserialize the payload)
                // https://github.com/umbraco/Umbraco-CMS/blob/release-7.4.0/src/Umbraco.Web/Cache/DataTypeCacheRefresher.cs#L64-L68
                var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int) } });
                if (payload == null)
                    return;

                foreach (var item in payload)
                {
                    applicationContext.ApplicationCache.RuntimeCache.ClearCacheItem(string.Format(InnerContentConstants.PreValuesCacheKey, item.Id));
                }
            };

            ContentTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType != MessageType.RefreshByJson)
                    return;

                // NOTE: The properties for the JSON payload are available here: (Currently there isn't a public API to deserialize the payload)
                // https://github.com/umbraco/Umbraco-CMS/blob/release-7.4.0/src/Umbraco.Web/Cache/ContentTypeCacheRefresher.cs#L93-L109
                var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int), AliasChanged = default(bool) } });
                if (payload == null)
                    return;

                // Only update if the content-type alias has changed.
                var ids = payload.Where(x => x.AliasChanged).Select(x => x.Id).ToArray();
                if (ids.Length == 0)
                    return;

                var contentTypes = applicationContext.Services.ContentTypeService.GetAllContentTypes(ids);
                foreach (var contentType in contentTypes)
                {
                    ContentTypeCacheHelper.TryRemove(contentType);
                    ContentTypeCacheHelper.TryAdd(contentType);
                }
            };
        }
    }
}