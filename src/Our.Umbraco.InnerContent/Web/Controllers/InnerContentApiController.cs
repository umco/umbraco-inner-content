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
            var contentTypes = Services.ContentTypeService.GetAllContentTypes(guids).OrderBy(x => x.SortOrder).ToList();
            var blueprints = Services.ContentService.GetBlueprintsForContentTypes(contentTypes.Select(x => x.Id).ToArray()).ToArray();

            // NOTE: Using an anonymous class, as the `ContentTypeBasic` type is heavier than what we need (for our requirements)
            return contentTypes.Select(ct => new
            {
                name = ct.Name, // TODO: localize the name (in case of dictionary items), e.g. `localizedTextService.UmbracoDictionaryTranslate`
                description = ct.Description, // TODO: localize the description (in case of dictionary items), e.g. `localizedTextService.UmbracoDictionaryTranslate`
                guid = ct.Key,
                key = ct.Key,
                icon = string.IsNullOrWhiteSpace(ct.Icon) || ct.Icon == ".sprTreeFolder" ? "icon-document" : ct.Icon,
                blueprints = blueprints.Where(bp => bp.ContentTypeId == ct.Id).ToDictionary(bp => bp.Id, bp => bp.Name)
                // TODO: tabs = ct.CompositionPropertyGroups.Select(y => y.Name).Distinct()
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

        [HttpGet]
        [UseInternalActionFilter("Umbraco.Web.WebApi.Filters.OutgoingEditorModelEventAttribute", onActionExecuted: true)]
        public ContentItemDisplay GetContentTypeScaffoldByBlueprintId(int blueprintId)
        {
            return new ContentController().GetEmpty(blueprintId, -20);
        }
    }
}