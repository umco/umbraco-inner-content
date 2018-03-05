using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.PropertyEditors
{
    public abstract class SimpleInnerContentPropertyEditor : PropertyEditor
    {
        private IDictionary<string, object> defaultPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return this.defaultPreValues; }
            set { this.defaultPreValues = value; }
        }

        public SimpleInnerContentPropertyEditor()
            : base()
        {
            this.defaultPreValues = new Dictionary<string, object>
            {
                { InnerContentConstants.ContentTypesPreValueKey, string.Empty }
            };
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new SimpleInnerContentPreValueEditor();
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new SimpleInnerContentPropertyValueEditor(base.CreateValueEditor());
        }
    }
}