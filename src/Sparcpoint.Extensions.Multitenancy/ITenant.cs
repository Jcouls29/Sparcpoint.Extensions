﻿namespace Sparcpoint.Extensions.Multitenancy;

public interface ITenant<out TTenant>
{
    TTenant Value { get; }
}
