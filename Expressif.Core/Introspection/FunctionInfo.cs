using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Accumulation;

namespace Expressif.Introspection;

public record FunctionInfo
(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    string Input,
    string Output,
    bool Converted,
    string Reason,
    Type ImplementationType,
    string Summary,
    ParameterInfo[] Parameters,
    bool Deprecated,
    string? Replacement,
    string? Sunset,
    bool ReplacementIsEquivalent,
    string? MigrationNotes,
    IReadOnlyList<TupleBindingSignature> Signatures
)
{
    public string Kind => typeof(IAccumulator).IsAssignableFrom(ImplementationType) ? "accumulator" : "function";
    public FunctionAliasLifecycleInfo[] DeprecatedAliases { get; init; } = [];
}

public sealed record FunctionAliasLifecycleInfo(
    string Name,
    string Replacement,
    string Message,
    string? Sunset = null,
    bool ReplacementIsEquivalent = true);

public record ParameterInfo
(
    string Name,
    string Type,
    bool Optional,
    bool Variadic,
    int MinimumCardinality,
    string Summary
);
