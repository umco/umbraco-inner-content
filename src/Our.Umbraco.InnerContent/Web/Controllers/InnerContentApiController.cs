using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Our.Umbraco.InnerContent.Web.WebApi.Filters;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Services;
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
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = TranslateItem(x.Name),
                    alias = x.Alias,
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [HttpGet]
        public IEnumerable<object> GetContentTypesByGuid([ModelBinder] Guid[] guids)
        {
            var contentTypes = Services.ContentTypeService.GetAllContentTypes(guids).OrderBy(x => Array.IndexOf(guids, x.Key)).ToList();
            var blueprints = Services.ContentService.GetBlueprintsForContentTypes(contentTypes.Select(x => x.Id).ToArray()).ToArray();

            // NOTE: Using an anonymous class, as the `ContentTypeBasic` type is heavier than what we need (for our requirements)
            return contentTypes.Select(ct => new
            {
                // TODO: localize the name and description (in case of dictionary items)
                // Umbraco core uses `localizedTextService.UmbracoDictionaryTranslate`, but this is currently marked as internal.
                // https://github.com/umbraco/Umbraco-CMS/blob/release-7.7.0/src/Umbraco.Core/Services/LocalizedTextServiceExtensions.cs#L76

                name = TranslateItem(ct.Name),
                description = ct.Description,
                guid = ct.Key,
                key = ct.Key,
                icon = string.IsNullOrWhiteSpace(ct.Icon) || ct.Icon == ".sprTreeFolder" ? "icon-document" : ct.Icon,
                blueprints = blueprints.Where(bp => bp.ContentTypeId == ct.Id).ToDictionary(bp => bp.Id, bp => bp.Name)
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
                    name = TranslateItem(x.Name),
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

        [HttpPost]
        public SimpleNotificationModel CreateBlueprintFromContent([FromBody] JObject item, int userId = 0)
        {
            var blueprint = InnerContentHelper.ConvertInnerContentToBlueprint(item, userId);

            Services.ContentService.SaveBlueprint(blueprint, userId);

            return new SimpleNotificationModel(new Notification(
                Services.TextService.Localize("blueprints/createdBlueprintHeading"),
                Services.TextService.Localize("blueprints/createdBlueprintMessage", new[] { blueprint.Name }),
                global::Umbraco.Web.UI.SpeechBubbleIcon.Success));
        }

        private static ICultureDictionary _cultureDictionary;
        private static ICultureDictionary CultureDictionary
        {
            get
            {
                return
                    _cultureDictionary ??
                    (_cultureDictionary = CultureDictionaryFactoryResolver.Current.Factory.CreateDictionary());
            }
        }

        public string TranslateItem(string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.StartsWith("#") == false)
                return text;

            text = text.Substring(1);
            return CultureDictionary[text].IfNullOrWhiteSpace(text);
        }
    }
}