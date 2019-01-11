using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.InnerContent.Models
{
    // NOTE: If Umbraco's `DetachedPublishedProperty` isn't currently publicly available.
    // https://github.com/umbraco/Umbraco-CMS/blob/release-7.7.0/src/Umbraco.Web/Models/DetachedPublishedProperty.cs#L7
    public class DetachedPublishedProperty : IPublishedProperty
    {
        private readonly PublishedPropertyType _propertyType;
        private readonly object _rawValue;
        private readonly Lazy<object> _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;
        private readonly bool _isPreview;

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value)
            : this(propertyType, value, false)
        { }

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value, bool isPreview)
        {
            _propertyType = propertyType;
            _isPreview = isPreview;

            _rawValue = value;

            _sourceValue = new Lazy<object>(() => _propertyType.ConvertDataToSource(_rawValue, _isPreview));
            _objectValue = new Lazy<object>(() => _propertyType.ConvertSourceToObject(_sourceValue.Value, _isPreview));
            _xpathValue = new Lazy<object>(() => _propertyType.ConvertSourceToXPath(_sourceValue.Value, _isPreview));
        }

        public string PropertyTypeAlias => _propertyType.PropertyTypeAlias;

        public bool HasValue
        {
            get { return DataValue != null && DataValue.ToString().Trim().Length > 0; }
        }

        public object DataValue => _rawValue;

        public object Value => _objectValue.Value;

        public object XPathValue => _xpathValue.Value;
    }
}