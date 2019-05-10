using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public abstract class SimpleInnerContentDataEditor : DataEditor
    {
        public SimpleInnerContentDataEditor(ILogger logger, EditorType type = EditorType.PropertyValue)
            : base(logger, type)
        {
            DefaultConfiguration.Add(InnerContentConstants.ContentTypesPreValueKey, string.Empty);
            DefaultConfiguration.Add(InnerContentConstants.EnableFilterPreValueKey, false);
        }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new SimpleInnerContentConfigurationEditor();
        }

        protected override IDataValueEditor CreateValueEditor()
        {
            return new SimpleInnerContentValueEditor(Attribute);
        }
    }
}