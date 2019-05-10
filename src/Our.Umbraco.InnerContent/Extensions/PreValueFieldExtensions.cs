//using System.Collections.Generic;
//using Umbraco.Core.PropertyEditors;

//namespace Our.Umbraco.InnerContent.PropertyEditors
//{
//    // TODO: Implement later [LK:2019-04-02]
//    public static class PreValueFieldExtensions
//    {
//        public static void Add(this List<PreValueField> fields, string key, string name, string view, string description)
//        {
//            fields.Add(new PreValueField
//            {
//                Key = key,
//                Name = name,
//                View = view,
//                Description = description
//            });
//        }

//        public static void AddHideLabel(this List<PreValueField> fields)
//        {
//            fields.Add(new PreValueField
//            {
//                Key = InnerContentConstants.HideLabelPreValueKey,
//                Name = "Hide Label",
//                View = "boolean",
//                Description = "Set whether to hide the editor label and have the list take up the full width of the editor window."
//            });
//        }
//    }
//}