using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Our.Umbraco.InnerContent.Web.WebApi.Filters;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.InnerContent.Web.Controllers
{
    [PluginController("InnerContent")]
    public class InnerContentApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public IEnumerable<object> GetAllContentTypes()
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = x.Name,
                    alias = x.Alias,
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [HttpGet]
        public IEnumerable<object> GetContentTypesByGuid([ModelBinder] Guid[] guids)
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .Where(x => guids == null || guids.Contains(x.Key))
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = x.Name,
                    alias = x.Alias,
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [HttpGet]
        public IEnumerable<object> GetContentTypesByAlias([ModelBinder] string[] aliases)
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
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [HttpGet]
        public IDictionary<string, string> GetContentTypeIconsByGuid([ModelBinder] Guid[] guids)
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .Where(x => guids.Contains(x.Key))
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon);
        }

        [HttpGet]
        [UseInternalActionFilter("Umbraco.Web.WebApi.Filters.OutgoingEditorModelEventAttribute", onActionExecuted: true)]
        public ContentItemDisplay GetContentTypeScaffoldByGuid(Guid guid)
        {
            var contentType = Services.ContentTypeService.GetContentType(guid);
            return new ContentController().GetEmpty(contentType.Alias, -20);
        }
    }
}