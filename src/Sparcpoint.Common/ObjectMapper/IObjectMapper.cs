using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Common;

public interface IObjectMapper
{
    IReadOnlyDictionary<string, string?> Map<T>(T obj);
    T? Map<T>(IReadOnlyDictionary<string, string?> dictionary);
}
