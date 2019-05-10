using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Umbraco.InnerContent
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class InnerContentComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components()
                .Append<ContentTypeCacheRefresherComponent>()
                .Append<DataTypeCacheRefresherComponent>();
        }
    }
}