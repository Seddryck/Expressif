using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Introspection;

namespace Expressif.Predicates.Introspection;

public record PredicateInfo
(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    Type ImplementationType,
    string Summary,
    ParameterInfo[] Parameters
);
