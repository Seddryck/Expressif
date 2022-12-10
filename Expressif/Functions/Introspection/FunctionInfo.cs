using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Introspection
{
    public record FunctionInfo
    (
        string Name,
        string[] Aliases,
        string Scope,
        Type ImplementationType
    );
}
