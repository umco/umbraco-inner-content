using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Web.Editors;

namespace Our.Umbraco.InnerContent.Web.Controllers
{
    public class InnerContentBaseController : UmbracoAuthorizedJsonController
    {
        private static ICultureDictionary _cultureDictionary;
        private static ICultureDictionary CultureDictionary
        {
            get
            {
                return
                    _cultureDictionary ??
                    (_cultureDictionary = CultureDictionaryFactoryResolver.Current.Factory.CreateDictionary());
            }
        }

        public string TranslateItem(string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.StartsWith("#") == false)
                return text;

            text = text.Substring(1);
            return CultureDictionary[text].IfNullOrWhiteSpace(text);
        }
    }
}
