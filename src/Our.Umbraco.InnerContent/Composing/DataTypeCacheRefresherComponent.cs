using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Our.Umbraco.InnerContent
{
    public class DataTypeCacheRefresherComponent : IComponent
    {
        public void Initialize()
        {
            DataTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType != MessageType.RefreshByJson)
                    return;

                // NOTE: The properties for the JSON payload are available here: (Currently there isn't a public API to deserialize the payload)
                // https://github.com/umbraco/Umbraco-CMS/blob/release-7.7.0/src/Umbraco.Web/Cache/DataTypeCacheRefresher.cs#L66-L70
                var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int) } });
                if (payload == null)
                    return;

                foreach (var item in payload)
                {
                    Current.AppCaches.RuntimeCache.Clear(string.Format(InnerContentConstants.PreValuesCacheKey, item.Id));
                }
            };
        }

        public void Terminate()
        { }
    }
}