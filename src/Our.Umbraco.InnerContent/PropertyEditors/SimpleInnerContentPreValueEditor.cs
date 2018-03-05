using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class SimpleInnerContentPreValueEditor : InnerContentPreValueEditor
    {
        public SimpleInnerContentPreValueEditor()
            : base()
        {
            // This ensures that the "contentTypes" field is always at the top of the prevalue fields.
            Fields.Insert(0, new PreValueField
            {
                Key = InnerContentConstants.ContentTypesPreValueKey,
                Name = "Content Types",
                View = IOHelper.ResolveUrl("~/App_Plugins/InnerContent/views/innercontent.doctypepicker.html"),
                Description = "Select the content types to use as the data blueprint."
            });
        }

        public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            if (persistedPreVals.IsDictionaryBased)
            {
                var dict = persistedPreVals.PreValuesAsDictionary;
                if (dict.TryGetValue(InnerContentConstants.ContentTypesPreValueKey, out PreValue contentTypes) && string.IsNullOrWhiteSpace(contentTypes.Value) == false)
                {
                    var items = JArray.Parse(contentTypes.Value);
                    if (TryEnsureContentTypeGuids(items))
                    {
                        contentTypes.Value = items.ToString();
                    }
                }
            }

            return base.ConvertDbToEditor(defaultPreVals, persistedPreVals);
        }
    }
}