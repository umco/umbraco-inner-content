using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public class SimpleInnerContentPreValueEditor : InnerContentPreValueEditor
    {
        [PreValueField("contentTypes", "Content Types", "~/App_Plugins/InnerContent/views/innercontent.doctypepicker.html", Description = "Select the content types to use as the data blueprint.")]
        public string[] ContentTypes { get; set; }

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