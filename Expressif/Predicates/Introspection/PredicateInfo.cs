using Expressif.Functions.Introspection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Introspection
{
    public record PredicateInfo
    (
        string Name,
        string[] Aliases,
        string Scope,
        Type ImplementationType,
        string Summary,
        ParameterInfo[] Parameters
    );
}
