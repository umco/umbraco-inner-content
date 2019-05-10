using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class SimpleInnerContentConfigurationEditor : InnerContentConfigurationEditor
    {
        public SimpleInnerContentConfigurationEditor()
            : base()
        {
            // This ensures that the "contentTypes" and "enableFilter" fields are always at the top of the prevalue fields.
            Fields.InsertRange(0, new[]
            {
                new ConfigurationField
                {
                    Key = InnerContentConstants.ContentTypesPreValueKey,
                    Name = "Content Types",
                    View = IOHelper.ResolveUrl("~/App_Plugins/InnerContent/views/innercontent.doctypepicker.html"),
                    Description = "Select the content types to use as the data blueprint."
                },
                new ConfigurationField
                {
                    Key = InnerContentConstants.EnableFilterPreValueKey,
                    Name = "Enable Filter?",
                    View = "boolean",
                    Description = "Select to enable a filter bar at the top of the Content Type selection."
                }
            });
        }

        public override object FromDatabase(string configurationJson)
        {
            var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(configurationJson);

            if (config.TryGetValue(InnerContentConstants.ContentTypesPreValueKey, out object val) && val is string contentTypes && string.IsNullOrWhiteSpace(contentTypes) == false)
            {
                var items = JArray.Parse(contentTypes);
                if (TryEnsureContentTypeGuids(items))
                {
                    config[InnerContentConstants.ContentTypesPreValueKey] = items.ToString();
                }
            }

            return config;
        }
    }
}