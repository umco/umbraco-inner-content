using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Our.Umbraco.InnerContent.Helpers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class SimpleInnerContentPreValueEditor : InnerContentPreValueEditor
    {
        [PreValueField("contentTypes", "Content Types", "~/App_Plugins/InnerContent/views/innercontent.doctypepicker.html", Description = "Select the content types to use as the data blueprint.")]
        public string[] ContentTypes { get; set; }

        [PreValueField("maxItems", "Max Items", "number", Description = "Set the maximum number of items allowed in this stack.")]
        public string MaxItems { get; set; }

        [PreValueField("singleItemMode", "Single Item Mode", "boolean", Description = "Set whether to work in single item mode (only the first defined Content Type will be used).")]
        public string SingleItemMode { get; set; }

        [PreValueField("hideLabel", "Hide Label", "boolean", Description = "Set whether to hide the editor label and have the list take up the full width of the editor window.")]
        public string HideLabel { get; set; }

        [PreValueField("disablePreview", "Disable Preview", "boolean", Description = "Set whether to disable the preview of the items in the stack.")]
        public string DisablePreview { get; set; }

        public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            if (persistedPreVals.IsDictionaryBased)
            {
                var dict = persistedPreVals.PreValuesAsDictionary;
                if (dict.TryGetValue("contentTypes", out PreValue contentTypes) && string.IsNullOrWhiteSpace(contentTypes.Value) == false)
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