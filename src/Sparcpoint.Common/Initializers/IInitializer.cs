using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Common.Initializers;

public interface IInitializer
{
    Task InitializeAsync();
}
