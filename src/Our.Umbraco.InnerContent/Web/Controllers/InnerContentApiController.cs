using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.InnerContent.Web.Controllers
{
    [PluginController("InnerContent")]
    public class InnerContentApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public IEnumerable<object> GetContentTypes()
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = x.Name,
                    alias = x.Alias,
                    icon = x.Icon.IsNullOrWhiteSpace() || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [HttpGet]
        public IEnumerable<object> GetContentTypeInfos([ModelBinder] string[] aliases)
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .Where(x => aliases == null || aliases.Contains(x.Alias))
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = x.Name,
                    alias = x.Alias,
                    icon = x.Icon.IsNullOrWhiteSpace() || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon
                });
        }

        [HttpGet]
        public IDictionary<string, string> GetContentTypeIcons([ModelBinder] string[] aliases)
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .Where(x => aliases.Contains(x.Alias))
                .ToDictionary(
                    x => x.Alias, 
                    x => x.Icon.IsNullOrWhiteSpace() || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon);
        }
    }
}
