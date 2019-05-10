using System.Linq;
using Newtonsoft.Json;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Our.Umbraco.InnerContent
{
    public class ContentTypeCacheRefresherComponent : IComponent
    {
        private readonly IContentTypeService _contentTypeService;

        public ContentTypeCacheRefresherComponent(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public void Initialize()
        {
            ContentTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType != MessageType.RefreshByJson)
                    return;

                // NOTE: The properties for the JSON payload are available here: (Currently there isn't a public API to deserialize the payload)
                // https://github.com/umbraco/Umbraco-CMS/blob/release-7.7.0/src/Umbraco.Web/Cache/ContentTypeCacheRefresher.cs#L91-L109
                var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int), AliasChanged = default(bool) } });
                if (payload == null)
                    return;

                // Only update if the content-type alias has changed.
                var ids = payload.Where(x => x.AliasChanged).Select(x => x.Id).ToArray();
                if (ids.Length == 0)
                    return;

                var contentTypes = _contentTypeService.GetAll(ids);
                foreach (var contentType in contentTypes)
                {
                    ContentTypeCacheHelper.TryRemove(contentType);
                    ContentTypeCacheHelper.TryAdd(contentType);
                }
            };

        }

        public void Terminate()
        { }
    }
}