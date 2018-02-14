using System.Linq;
using Newtonsoft.Json;
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
                if (e.MessageType == MessageType.RefreshByJson)
                {
                    var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int) } });
                    if (payload != null)
                    {
                        foreach (var item in payload)
                        {
                            applicationContext.ApplicationCache.RuntimeCache.ClearCacheItem(
                                string.Format(InnerContentConstants.PreValuesCacheKey, item.Id));
                        }
                    }
                }
            };

            ContentTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType == MessageType.RefreshByJson)
                {
                    var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int) } });
                    if (payload != null)
                    {
                        var contentTypes = applicationContext.Services.ContentTypeService.GetAllContentTypes(payload.Select(x => x.Id).ToArray());
                        foreach (var contentType in contentTypes)
                        {
                            var key = string.Format(InnerContentConstants.ContentTypeAliasByGuidCacheKey, contentType.Key);
                            var alias = applicationContext.ApplicationCache.StaticCache.GetCacheItem(key, () => contentType.Alias);
                            if (alias != null && alias.ToString() != contentType.Alias)
                            {
                                applicationContext.ApplicationCache.StaticCache.ClearCacheItem(key);
                                applicationContext.ApplicationCache.StaticCache.GetCacheItem(key, () => contentType.Alias);
                            }
                        }
                    }
                }
            };
        }
    }
}