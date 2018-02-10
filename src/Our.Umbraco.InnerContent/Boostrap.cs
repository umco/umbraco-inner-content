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
        }
    }
}