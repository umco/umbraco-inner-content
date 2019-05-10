using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.InnerContent.Models
{
    public class DetachedPublishedElement : IPublishedElement
    {
        public DetachedPublishedElement(Guid key, string name, IPublishedContentType contentType, IEnumerable<IPublishedProperty> properties)
        {
            Key = key;
            ContentType = contentType;
            Properties = properties;
        }

        public IPublishedContentType ContentType { get; }

        public Guid Key { get; }

        public IEnumerable<IPublishedProperty> Properties { get; }

        public IPublishedProperty GetProperty(string alias)
        {
            // TODO: See how v8/NC does this [LK:2019-04-02]
            // https://github.com/umbraco/Umbraco-CMS/blob/v8/dev/src/Umbraco.Web/PublishedCache/PublishedElement.cs#L79-L84

            return Properties.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }
    }
}