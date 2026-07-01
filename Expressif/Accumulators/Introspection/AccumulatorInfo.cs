using System;
using Expressif.Functions.Introspection;

namespace Expressif.Accumulators.Introspection;

public record AccumulatorInfo
(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    Type ImplementationType,
    string Summary,
    ParameterInfo[] Parameters
);
