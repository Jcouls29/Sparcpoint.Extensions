using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Common;

public interface IObjectMapper
{
    void Map<T>(T obj, IDictionary<string, string?> output);
    void Map<T>(IReadOnlyDictionary<string, string?> dictionary, T output);
}
