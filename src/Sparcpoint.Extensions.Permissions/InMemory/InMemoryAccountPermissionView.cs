﻿using Sparcpoint.Extensions.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryAccountPermissionView : IAccountPermissionView
{
    private readonly List<AccountPermissionEntry> _Entries;

    public InMemoryAccountPermissionView(List<AccountPermissionEntry> entries, string accountId, ScopePath currentScope)
    {
        _Entries = entries;
        AccountId = accountId;
        CurrentScope = currentScope;
    }

    public string AccountId { get; }
    public ScopePath CurrentScope { get; }

    public IAsyncEnumerator<PermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var intermediate = _Entries.Where(e => e.AccountId == AccountId && e.Entry.Scope <= CurrentScope).Select(c => c.Entry).ToArray();
        var allPermissionKeys = intermediate.Select(c => c.Key).Distinct();

        var view = allPermissionKeys.Select(k => new PermissionEntry(k, intermediate.CalculatePermissionValue(CurrentScope, k), CurrentScope, null)).ToList();
        return new SynchronousAsyncEnumerator<PermissionEntry>(view.GetEnumerator());
    }
}