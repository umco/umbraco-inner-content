using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public abstract class InnerContentValueEditor : DataValueEditor
    {
        public InnerContentValueEditor(DataEditorAttribute attribute)
            : base(attribute)
        {
            // TODO: Parse out the configuration and check the "hide label" flag. [LK:2019-04-02]
            // `InnerContentConstants.HideLabelPreValueKey === "1"`
            if (Configuration != null)
            {
                HideLabel = false; // TODO: Implement later! [LK:2019-04-02]
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
                        //// Create a fake property using the property abd stored value
                        //var prop = new Property(propType);
                        //prop.SetValue(item[propKey]?.ToString());  

                        // Lookup the property editor
                        if (Current.PropertyEditors.TryGet(propType.PropertyEditorAlias, out IDataEditor propEditor))
                        {
                            // Get the editor to do it's conversion, and store it back

                            // TODO: Clarify if this is the correct way of doing this? [LK:2019-04-02]
                            var valueEditor = propEditor.GetValueEditor();
                            item[propKey] = valueEditor?.ConvertDbToString(propType, item[propKey]?.ToString(), dataTypeService);
                        }
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
                var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propKey));
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
                        var prop = new Property(propType);
                        prop.SetValue(item[propKey]?.ToString()); // TODO: Check if this is the correct way to do this? Confused about the culture/segment bits. [LK:2019-04-02]

                        // Lookup the property editor
                        if (Current.PropertyEditors.TryGet(propType.PropertyEditorAlias, out IDataEditor propEditor))
                        {

                            // Get the editor to do it's conversion
                            var valueEditor = propEditor.GetValueEditor();
                            var newValue = valueEditor?.ToEditor(prop, dataTypeService);

                            // Store the value back
                            item[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                        }
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
            var childrenProp = item.Properties().FirstOrDefault(x => x.Name.InvariantEquals("children"));
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
                    var propPreValues = dataTypeService.GetDataType(propType.DataTypeId).Configuration;

                    // Lookup the property editor
                    if (Current.PropertyEditors.TryGet(propType.PropertyEditorAlias, out IDataEditor propEditor))
                    {
                        // Create a fake content property data object
                        var contentPropData = new ContentPropertyData(item[propKey], propPreValues);

                        // Get the property editor to do it's conversion
                        var valueEditor = propEditor.GetValueEditor(); // TODO: Clarify this is correct. [LK:2019-02-04]
                        var newValue = valueEditor?.FromEditor(contentPropData, item[propKey]);

                        // Store the value back
                        item[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                    }
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