using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class SimpleInnerContentPropertyValueEditor : InnerContentPropertyValueEditor
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
            ConvertDbToStringRecursive(value, dataTypeService);

            // Return the serialized value
            return JsonConvert.SerializeObject(value);
        }

        protected void ConvertDbToStringRecursive(JToken token, IDataTypeService dataTypeService)
        {
            if (token is JArray jArr)
            {
                foreach (var item in jArr)
                {
                    ConvertDbToStringRecursive(item, dataTypeService);
                }
            }

            if (token is JObject jObj)
            {
                if (jObj[InnerContentConstants.ContentTypeGuidPropertyKey] != null || jObj[InnerContentConstants.ContentTypeAliasPropertyKey] != null)
                {
                    ConvertInnerContentDbToString(jObj, dataTypeService);
                }
                else
                {
                    foreach (var kvp in jObj)
                    {
                        if (kvp.Value is JArray || kvp.Value is JObject)
                        {
                            ConvertDbToStringRecursive(kvp.Value, dataTypeService);
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
            ConvertDbToEditorRecursive(value, dataTypeService);

            // Return the JObject, Angular can handle it directly
            return value;
        }

        protected void ConvertDbToEditorRecursive(JToken token, IDataTypeService dataTypeService)
        {
            if (token is JArray jArr)
            {
                foreach (var item in jArr)
                {
                    ConvertDbToEditorRecursive(item, dataTypeService);
                }
            }

            if (token is JObject jObj)
            {
                if (jObj[InnerContentConstants.ContentTypeGuidPropertyKey] != null || jObj[InnerContentConstants.ContentTypeAliasPropertyKey] != null)
                {
                    ConvertInnerContentDbToEditor(jObj, dataTypeService);
                }
                else
                {
                    foreach (var kvp in jObj)
                    {
                        if (kvp.Value is JArray || kvp.Value is JObject)
                        {
                            ConvertDbToEditorRecursive(kvp.Value, dataTypeService);
                        }
                    }
                }
            }
        }

        public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            // Convert / validate value
            if (editorValue.Value == null)
                return string.Empty;

            var dbValue = editorValue.Value.ToString();
            if (string.IsNullOrWhiteSpace(dbValue))
                return string.Empty;

            var value = JsonConvert.DeserializeObject<JToken>(dbValue);
            if (value == null || (value is JArray && ((JArray)value).Count == 0))
                return string.Empty;

            // Process value
            ConvertEditorToDbRecursive(value, currentValue);

            // Return value
            return JsonConvert.SerializeObject(value);
        }

        protected void ConvertEditorToDbRecursive(JToken token, object currentValue)
        {
            if (token is JArray jArr)
            {
                foreach (var item in jArr)
                {
                    ConvertEditorToDbRecursive(item, currentValue);
                }
            }

            if (token is JObject jObj)
            {
                if (jObj[InnerContentConstants.ContentTypeGuidPropertyKey] != null || jObj[InnerContentConstants.ContentTypeAliasPropertyKey] != null)
                {
                    ConvertInnerContentEditorToDb(jObj, ApplicationContext.Current.Services.DataTypeService);
                }
                else
                {
                    foreach (var kvp in jObj)
                    {
                        if (kvp.Value is JArray || kvp.Value is JObject)
                        {
                            ConvertEditorToDbRecursive(kvp.Value, currentValue);
                        }
                    }
                }
            }
        }
    }
}