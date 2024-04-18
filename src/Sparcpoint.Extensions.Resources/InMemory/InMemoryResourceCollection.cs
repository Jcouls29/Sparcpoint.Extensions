using System.Collections.Concurrent;

namespace Sparcpoint.Extensions.Resources.InMemory;

internal class InMemoryResourceCollection : ConcurrentDictionary<ScopePath, InMemoryResource> { }
