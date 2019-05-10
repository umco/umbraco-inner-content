using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.InnerContent.Models
{
    public class DetachedPublishedProperty : IPublishedProperty
    {
        private readonly object _sourceValue;
        private readonly Lazy<object> _interValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;

        public DetachedPublishedProperty(IPublishedPropertyType propertyType, IPublishedElement owner, object value)
            : this(propertyType, owner, value, false)
        { }

        public DetachedPublishedProperty(IPublishedPropertyType propertyType, IPublishedElement owner, object value, bool preview)
        {
            PropertyType = propertyType;

            _sourceValue = value;

            _interValue = new Lazy<object>(() => PropertyType.ConvertSourceToInter(owner, _sourceValue, preview));
            _objectValue = new Lazy<object>(() => PropertyType.ConvertInterToObject(owner, PropertyCacheLevel.Unknown, _interValue.Value, preview));
            _xpathValue = new Lazy<object>(() => PropertyType.ConvertInterToXPath(owner, PropertyCacheLevel.Unknown, _interValue.Value, preview));
        }

        public IPublishedPropertyType PropertyType { get; }

        public string Alias => PropertyType.Alias;

        public object GetSourceValue(string culture = null, string segment = null)
        {
            return _sourceValue;
        }

        public object GetValue(string culture = null, string segment = null)
        {
            return _objectValue.Value;
        }

        public object GetXPathValue(string culture = null, string segment = null)
        {
            return _xpathValue.Value;
        }

        public bool HasValue(string culture = null, string segment = null)
        {
            return _sourceValue != null && _sourceValue.ToString().Trim().Length > 0;
        }
    }
}