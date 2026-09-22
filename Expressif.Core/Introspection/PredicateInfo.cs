using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Discovery;

namespace Expressif.Introspection;

public record PredicateInfo
(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    Type ImplementationType,
    string Summary,
    ParameterInfo[] Parameters,
    IReadOnlyList<TupleBindingSignature> Signatures
);
