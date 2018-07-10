using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public abstract class InnerContentPropertyValueEditor : PropertyValueEditorWrapper
    {
        protected InnerContentPropertyValueEditor(PropertyValueEditor wrapped)
            : base(wrapped)
        { }

        public override void ConfigureForDisplay(PreValueCollection preValues)
        {
            base.ConfigureForDisplay(preValues);

            var asDictionary = preValues.PreValuesAsDictionary;
            if (asDictionary.ContainsKey("hideLabel"))
            {
                var boolAttempt = asDictionary["hideLabel"].Value.TryConvertTo<bool>();
                if (boolAttempt.Success)
                {
                    HideLabel = boolAttempt.Result;
                }
            }
        }

        #region Db to String

        protected void ConvertInnerContentDbToString(JArray items, IDataTypeService dataTypeService)
        {
            foreach (var item in items)
            {
                ConvertInnerContentDbToString(item as JObject, dataTypeService);
            }
        }

        protected void ConvertInnerContentDbToString(JObject item, IDataTypeService dataTypeService)
        {
            if (item == null)
                return;

            var contentType = InnerContentHelper.GetContentTypeFromItem(item);
            if (contentType == null)
                return;

            var propValueKeys = item.Properties().Select(x => x.Name).ToArray();

            foreach (var propKey in propValueKeys)
            {
                var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                if (propType == null)
                {
                    if (IsSystemPropertyKey(propKey) == false)
                    {
                        // Property missing so just delete the value
                        item[propKey] = null;
                    }
                }
                else
                {
                    try
                    {
                        // Create a fake property using the property abd stored value
                        var prop = new Property(propType, item[propKey]?.ToString());

                        // Lookup the property editor
                        var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                        // Get the editor to do it's conversion, and store it back
                        item[propKey] = propEditor.ValueEditor.ConvertDbToString(prop, propType, dataTypeService);
                    }
                    catch (InvalidOperationException)
                    {
                        // https://github.com/umco/umbraco-nested-content/issues/111
                        // Catch any invalid cast operations as likely means Courier failed due to missing
                        // or trashed item so couldn't convert a guid back to an int

                        item[propKey] = null;
                    }
                }
            }

            // Process children
            var childrenProp = item.Properties().FirstOrDefault(x => x.Name == "children");
            if (childrenProp != null)
            {
                ConvertInnerContentDbToString(childrenProp.Value.Value<JArray>(), dataTypeService);
            }
        }

        #endregion

        #region DB to Editor

        protected void ConvertInnerContentDbToEditor(JArray items, IDataTypeService dataTypeService)
        {
            foreach (var item in items)
            {
                ConvertInnerContentDbToEditor(item as JObject, dataTypeService);
            }
        }

        protected void ConvertInnerContentDbToEditor(JObject item, IDataTypeService dataTypeService)
        {
            if (item == null)
                return;

            var contentType = InnerContentHelper.GetContentTypeFromItem(item);
            if (contentType == null)
                return;

            var propValueKeys = item.Properties().Select(x => x.Name).ToArray();

            foreach (var propKey in propValueKeys)
            {
                var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                if (propType == null)
                {
                    if (IsSystemPropertyKey(propKey) == false)
                    {
                        // Property missing so just delete the value
                        item[propKey] = null;
                    }
                }
                else
                {
                    try
                    {
                        // Create a fake property using the property abd stored value
                        var prop = new Property(propType, item[propKey]?.ToString());

                        // Lookup the property editor
                        var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                        // Get the editor to do it's conversion
                        var newValue = propEditor.ValueEditor.ConvertDbToEditor(prop, propType, dataTypeService);

                        // Store the value back
                        item[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                    }
                    catch (InvalidOperationException)
                    {
                        // https://github.com/umco/umbraco-nested-content/issues/111
                        // Catch any invalid cast operations as likely means courier failed due to missing
                        // or trashed item so couldn't convert a guid back to an int

                        item[propKey] = null;
                    }
                }
            }

            // Process children
            var childrenProp = item.Properties().FirstOrDefault(x => x.Name == "children");
            if (childrenProp != null)
            {
                ConvertInnerContentDbToEditor(childrenProp.Value.Value<JArray>(), dataTypeService);
            }
        }

        #endregion

        #region Editor to Db

        protected void ConvertInnerContentEditorToDb(JArray items, IDataTypeService dataTypeService)
        {
            foreach (var item in items)
            {
                ConvertInnerContentEditorToDb(item as JObject, dataTypeService);
            }
        }

        protected void ConvertInnerContentEditorToDb(JObject item, IDataTypeService dataTypeService)
        {
            if (item == null)
                return;

            var contentType = InnerContentHelper.GetContentTypeFromItem(item);
            if (contentType == null)
                return;

            var propValueKeys = item.Properties().Select(x => x.Name).ToArray();

            foreach (var propKey in propValueKeys)
            {
                var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                if (propType == null)
                {
                    if (IsSystemPropertyKey(propKey) == false)
                    {
                        // Property missing so just delete the value
                        item[propKey] = null;
                    }
                }
                else
                {
                    // Fetch the property types prevalue
                    var propPreValues = dataTypeService.GetPreValuesCollectionByDataTypeId(propType.DataTypeDefinitionId);

                    // Lookup the property editor
                    var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                    // Create a fake content property data object
                    var contentPropData = new ContentPropertyData(
                        item[propKey], propPreValues,
                        new Dictionary<string, object>());

                    // Get the property editor to do it's conversion
                    var newValue = propEditor.ValueEditor.ConvertEditorToDb(contentPropData, item[propKey]);

                    // Store the value back
                    item[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                }
            }

            // Process children
            var childrenProp = item.Properties().FirstOrDefault(x => x.Name == "children");
            if (childrenProp != null)
            {
                ConvertInnerContentEditorToDb(childrenProp.Value.Value<JArray>(), dataTypeService);
            }
        }

        #endregion

        #region Helpers

        private static bool IsSystemPropertyKey(string propKey)
        {
            return propKey == "name"
                || propKey == "children"
                || propKey == "key"
                || propKey == "icon"
                || propKey == InnerContentConstants.ContentTypeGuidPropertyKey
                || propKey == InnerContentConstants.ContentTypeAliasPropertyKey;
        }

        #endregion
    }
}