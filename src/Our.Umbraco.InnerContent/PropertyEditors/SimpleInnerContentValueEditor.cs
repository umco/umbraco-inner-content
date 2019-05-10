using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class SimpleInnerContentValueEditor : InnerContentValueEditor
    {
        public SimpleInnerContentValueEditor(DataEditorAttribute attribute)
            : base(attribute)
        { }

        public override string ConvertDbToString(PropertyType propertyType, object value, IDataTypeService dataTypeService)
        {
            // Convert / validate value
            if (value == null)
                return string.Empty;

            var propertyValue = value.ToString();
            if (string.IsNullOrWhiteSpace(propertyValue))
                return string.Empty;

            var token = JsonConvert.DeserializeObject<JToken>(propertyValue);
            if (token == null)
                return string.Empty;

            // Process value
            ConvertDbToStringRecursive(token, dataTypeService);

            // Return the serialized value
            return JsonConvert.SerializeObject(token);
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

        public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
        {
            var value = property.GetValue(culture, segment);

            // Convert / validate value
            if (value == null)
                return string.Empty;

            var propertyValue = value.ToString();
            if (string.IsNullOrWhiteSpace(propertyValue))
                return string.Empty;

            var token = JsonConvert.DeserializeObject<JToken>(propertyValue);
            if (token == null)
                return string.Empty;

            // Process value
            ConvertDbToEditorRecursive(token, dataTypeService);

            // Return the JObject, Angular can handle it directly
            return token;
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

        public override object FromEditor(ContentPropertyData editorValue, object currentValue)
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
                    ConvertInnerContentEditorToDb(jObj, Current.Services.DataTypeService);
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