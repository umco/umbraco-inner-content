using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.InnerContent.Web.Controllers
{
    [PluginController("InnerContent")]
    public class InnerContentApiController : UmbracoAuthorizedJsonController
    {
        [System.Web.Http.HttpGet]
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
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [System.Web.Http.HttpGet]
        public IEnumerable<object> GetContentTypeInfos([System.Web.Http.ModelBinding.ModelBinder] Guid[] guids)
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
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon
                });
        }

        [System.Web.Http.HttpGet]
        public IDictionary<string, string> GetContentTypeIcons([System.Web.Http.ModelBinding.ModelBinder] Guid[] guids)
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .Where(x => guids.Contains(x.Key))
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon);
        }

        [HttpGet]
        public ContentItemDisplay GetContentTypeScaffold(Guid guid)
        {
            var contentType = Services.ContentTypeService.GetContentType(guid);
            return new ContentController().GetEmpty(contentType.Alias, -20);
        }
    }
}