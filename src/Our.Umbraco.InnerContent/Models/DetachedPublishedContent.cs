using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using UmbracoWebModels = Umbraco.Web.Models;

namespace Our.Umbraco.InnerContent.Models
{
    // NOTE: Inherits from https://github.com/umbraco/Umbraco-CMS/blob/release-7.7.0/src/Umbraco.Web/Models/DetachedPublishedContent.cs
    // The main differences are that this one can set the Level, Children and Parent, (and includes a recursive `GetProperty` method).
    public class DetachedPublishedContent : UmbracoWebModels.DetachedPublishedContent
    {
        private readonly IPublishedContent _parentNode;
        private IEnumerable<IPublishedContent> _children;
        private readonly int _level;

        public DetachedPublishedContent(Guid key,
            string name,
            PublishedContentType contentType,
            IEnumerable<IPublishedProperty> properties,
            IPublishedContent containerNode = null,
            IPublishedContent parentNode = null,
            int sortOrder = 0,
            int level = 0,
            bool isPreviewing = false)
            : base(key, name, contentType, properties, containerNode, sortOrder, isPreviewing)
        {
            _level = level;
            _parentNode = parentNode;
            _children = Enumerable.Empty<IPublishedContent>();
        }

        public void SetChildren(IEnumerable<IPublishedContent> children)
        {
            _children = children;
        }

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var prop = GetProperty(alias);

            if (recurse && Parent != null && prop == null)
                return Parent.GetProperty(alias, true);

            return prop;
        }

        public override IPublishedContent Parent => _parentNode;

        public override IEnumerable<IPublishedContent> Children => _children;

        public override int Level => _level;
    }
}