using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sparcpoint.Extensions.Permissions.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryPermissions(this IServiceCollection services)
    {
        List<AccountPermissionEntry> entries = new List<AccountPermissionEntry>();

        services.TryAddSingleton<IPermissionStore>(new InMemoryPermissionStore(entries));

        return services;
    }
}
