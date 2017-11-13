using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.InnerContent
{
    public class Bootstrap : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // TODO: [LK] Replace with the distributed cache expiry
            DataTypeService.Saved += ExpireCache;
        }

        private void ExpireCache(IDataTypeService sender, SaveEventArgs<IDataTypeDefinition> e)
        {
            foreach (var dataType in e.SavedEntities)
            {
                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(
                    string.Concat("Our.Umbraco.InnerContent.GetPreValuesCollectionByDataTypeId_", dataType.Id));
            }
        }
    }
}