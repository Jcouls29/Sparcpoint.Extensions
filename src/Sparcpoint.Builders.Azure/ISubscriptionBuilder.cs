using System;

namespace Sparcpoint.Builders.Azure
{
    public interface ISubscriptionBuilder
    {
        ISubscriptionBuilder ResourceGroup(string resourceGroupName, Func<IResourceGroupBuilder, IResourceGroupBuilder> rg);
    }
}
