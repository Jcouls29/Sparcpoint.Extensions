using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sparcpoint.Abstractions.Internal
{
    internal class DefaultSparcpointServiceCollection : ISparcpointServiceCollection
    {
        public DefaultSparcpointServiceCollection(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }

}
