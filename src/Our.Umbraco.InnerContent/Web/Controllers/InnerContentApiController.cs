using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Our.Umbraco.InnerContent.Web.WebApi.Filters;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;

namespace Our.Umbraco.InnerContent.Web.Controllers
{
    [PluginController("InnerContent")]
    public class InnerContentApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public IEnumerable<object> GetAllContentTypes()
        {
            return Services
                .ContentTypeService
                .GetAll()
                .Where(x => x.IsElement)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = UmbracoDictionaryTranslate(x.Name),
                    alias = x.Alias,
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [HttpGet]
        public IEnumerable<object> GetContentTypesByGuid([ModelBinder] Guid[] guids)
        {
            var contentTypes = Services.ContentTypeService.GetAll().Where(x => guids.Contains(x.Key)).OrderBy(x => Array.IndexOf(guids, x.Key)).ToList();
            var blueprints = Services.ContentService.GetBlueprintsForContentTypes(contentTypes.Select(x => x.Id).ToArray()).ToArray();

            // NOTE: Using an anonymous class, as the `ContentTypeBasic` type is heavier than what we need (for our requirements)
            return contentTypes.Select(ct => new
            {
                name = UmbracoDictionaryTranslate(ct.Name),
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
            return Services.ContentTypeService.GetAll()
                .Where(x => aliases == null || aliases.Contains(x.Alias))
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = UmbracoDictionaryTranslate(x.Name),
                    alias = x.Alias,
                    icon = string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }

        [HttpGet]
        public IDictionary<string, string> GetContentTypeIconsByGuid([ModelBinder] Guid[] guids)
        {
            return Services.ContentTypeService.GetAll()
                .Where(x => guids.Contains(x.Key))
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => string.IsNullOrWhiteSpace(x.Icon) || x.Icon == ".sprTreeFolder" ? "icon-folder" : x.Icon);
        }

        [HttpGet]
        [UseInternalActionFilter("Umbraco.Web.WebApi.Filters.OutgoingEditorModelEventAttribute", onActionExecuted: true)]
        public ContentItemDisplay GetContentTypeScaffoldByGuid(Guid guid)
        {
            var contentType = Services.ContentTypeService.Get(guid);

            // TODO: I'll need to figure out how DependencyInjection works here - literally zero idea, without going overkill. [LK:2019-04-02]
            // https://github.com/umbraco/Umbraco-CMS/blob/v8/dev/src/Umbraco.Web/Editors/ContentController.cs#L349
            //return new ContentController().GetEmpty(contentType.Alias, -20);

            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = Services.ContentService.Create("", -20, contentType.Alias, Security.GetUserId().ResultOr(0));
            var mapped = MapToDisplay(emptyContent);

            // TODO: These bits are the reason I want to reuse Umbraco core code, not copy it to our project! [LK:2019-04-02]
            //// translate the content type name if applicable
            //mapped.ContentTypeName = Services.TextService.UmbracoDictionaryTranslate(mapped.ContentTypeName);

            //// if your user type doesn't have access to the Settings section it would not get this property mapped
            //if (mapped.DocumentType != null)
            //    mapped.DocumentType.Name = Services.TextService.UmbracoDictionaryTranslate(mapped.DocumentType.Name);

            //remove the listview app if it exists
            mapped.ContentApps = mapped.ContentApps.Where(x => x.Alias != "umbListView").ToList();

            return mapped;
        }

        [HttpGet]
        [UseInternalActionFilter("Umbraco.Web.WebApi.Filters.OutgoingEditorModelEventAttribute", onActionExecuted: true)]
        public ContentItemDisplay GetContentTypeScaffoldByBlueprintId(int blueprintId)
        {
            // TODO: I'll need to figure out how DependencyInjection works here - literally zero idea, without going overkill. [LK:2019-04-02]
            // https://github.com/umbraco/Umbraco-CMS/blob/v8/dev/src/Umbraco.Web/Editors/ContentController.cs#L372
            //return new ContentController().GetEmpty(blueprintId, -20);

            var blueprint = Services.ContentService.GetBlueprintById(blueprintId);
            if (blueprint == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            blueprint.Id = 0;
            blueprint.Name = string.Empty;
            blueprint.ParentId = -20;

            var mapped = Mapper.Map<ContentItemDisplay>(blueprint);

            //remove the listview app if it exists
            mapped.ContentApps = mapped.ContentApps.Where(x => x.Alias != "umbListView").ToList();

            return mapped;
        }

        [HttpPost]
        public SimpleNotificationModel CreateBlueprintFromContent([FromBody] JObject item, int userId = 0)
        {
            var blueprint = InnerContentHelper.ConvertInnerContentToBlueprint(item, userId);

            Services.ContentService.SaveBlueprint(blueprint, userId);

            return new SimpleNotificationModel(new Notification(
                Services.TextService.Localize("blueprints/createdBlueprintHeading"),
                Services.TextService.Localize("blueprints/createdBlueprintMessage", new[] { blueprint.Name }),
                NotificationStyle.Success));
        }

        // Umbraco core's `localizedTextService.UmbracoDictionaryTranslate` is internal. Until it's made public, we have to roll our own.
        // https://github.com/umbraco/Umbraco-CMS/blob/release-7.7.0/src/Umbraco.Core/Services/LocalizedTextServiceExtensions.cs#L76
        private string UmbracoDictionaryTranslate(string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.StartsWith("#") == false)
            {
                return text;
            }

            text = text.Substring(1);

            if (_cultureDictionary == null)
            {
                _cultureDictionary = Current.CultureDictionaryFactory.CreateDictionary();
            }

            return _cultureDictionary[text].IfNullOrWhiteSpace(text);
        }

        private static ICultureDictionary _cultureDictionary;

        // TODO: Figure out how to get rid of `MapToDisplay` [LK:2019-04-02]
        // Copied from here: https://github.com/umbraco/Umbraco-CMS/blob/v8/dev/src/Umbraco.Web/Editors/ContentController.cs#L2058
        private ContentItemDisplay MapToDisplay(IContent content)
        {
            var display = Mapper.Map<ContentItemDisplay>(content);
            display.AllowPreview = display.AllowPreview && content.Trashed == false && content.ContentType.IsElement == false;
            return display;
        }
    }
}