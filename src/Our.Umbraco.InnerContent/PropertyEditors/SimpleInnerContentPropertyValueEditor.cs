using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class SimpleInnerContentPropertyValueEditor : InnerContentPropertyValueEditorWrapper
    {
        public SimpleInnerContentPropertyValueEditor(PropertyValueEditor wrapped)
            : base(wrapped)
        { }

        public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            // Convert / validate value
            if (property.Value == null)
                return string.Empty;

            var propertyValue = property.Value.ToString();
            if (string.IsNullOrWhiteSpace(propertyValue))
                return string.Empty;

            var value = JsonConvert.DeserializeObject<JToken>(propertyValue);
            if (value == null)
                return string.Empty;

            // Process value
            ConvertDbToStringRecursive(value, property, propertyType, dataTypeService);

            // Update the value on the property
            property.Value = JsonConvert.SerializeObject(value);

            // Pass the call down
            return base.ConvertDbToString(property, propertyType, dataTypeService);
        }

        protected void ConvertDbToStringRecursive(JToken token, Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (token is JArray jArr)
            {
                foreach (var item in jArr)
                {
                    ConvertDbToStringRecursive(item, property, propertyType, dataTypeService);
                }
            }

            if (token is JObject jObj)
            {
                if (jObj[InnerContentConstants.ContentTypeAliasPropertyKey] != null)
                {
                    ConvertInnerContentDbToString(jObj);
                }
                else
                {
                    foreach (var kvp in jObj)
                    {
                        if (kvp.Value is JArray || kvp.Value is JObject)
                        {
                            ConvertDbToStringRecursive(kvp.Value, property, propertyType, dataTypeService);
                        }
                    }
                }
            }
        }

        public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            // Convert / validate value
            if (property.Value == null)
                return string.Empty;

            var propertyValue = property.Value.ToString();
            if (string.IsNullOrWhiteSpace(propertyValue))
                return string.Empty;

            var value = JsonConvert.DeserializeObject<JToken>(propertyValue);
            if (value == null)
                return string.Empty;

            // Process value
            ConvertDbToEditorRecursive(value, property, propertyType, dataTypeService);

            // Update the value on the property
            property.Value = JsonConvert.SerializeObject(value);

            // Pass the call down
            return base.ConvertDbToEditor(property, propertyType, dataTypeService);
        }

        protected void ConvertDbToEditorRecursive(JToken token, Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (token is JArray jArr)
            {
                foreach (var item in jArr)
                {
                    ConvertDbToEditorRecursive(item, property, propertyType, dataTypeService);
                }
            }

            if (token is JObject jObj)
            {
                if (jObj[InnerContentConstants.ContentTypeAliasPropertyKey] != null)
                {
                    ConvertInnerContentDbToEditor(jObj);
                }
                else
                {
                    foreach (var kvp in jObj)
                    {
                        if (kvp.Value is JArray || kvp.Value is JObject)
                        {
                            ConvertDbToEditorRecursive(kvp.Value, property, propertyType, dataTypeService);
                        }
                    }
                }
            }
        }

        public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            // Convert / validate value
            if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                return null;

            var value = JsonConvert.DeserializeObject<JToken>(editorValue.Value.ToString());
            if (value == null || (value is JArray && ((JArray)value).Count == 0))
                return null;

            // Process value
            ConvertEditorToDbRecursive(value, editorValue, currentValue);

            // Return value
            return JsonConvert.SerializeObject(value);
        }

        protected void ConvertEditorToDbRecursive(JToken token, ContentPropertyData editorValue, object currentValue)
        {
            if (token is JArray jArr)
            {
                foreach (var item in jArr)
                {
                    ConvertEditorToDbRecursive(item, editorValue, currentValue);
                }
            }

            if (token is JObject jObj)
            {
                if (jObj[InnerContentConstants.ContentTypeAliasPropertyKey] != null)
                {
                    ConvertInnerContentEditorToDb(jObj);
                }
                else
                {
                    foreach (var kvp in jObj)
                    {
                        if (kvp.Value is JArray || kvp.Value is JObject)
                        {
                            ConvertEditorToDbRecursive(kvp.Value, editorValue, currentValue);
                        }
                    }
                }
            }
        }
    }
}