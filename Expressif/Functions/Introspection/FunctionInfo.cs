using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Introspection;

public record FunctionInfo
(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    Type ImplementationType,
    string Summary,
    ParameterInfo[] Parameters
);

public record ParameterInfo
(
    string Name,
    bool Optional,
    string Summary
);
