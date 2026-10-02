using Expressif.Library.Catalog;
using Expressif.Discovery;
using Expressif.Functions.Accumulation;
using Expressif.Functions.Coercions;
using Expressif.Planning;
using Expressif.Values.Types;

namespace Expressif.Library.Composition;

/// <summary>
/// Composes Core logical planning with the official built-in Expressif vocabulary.
/// </summary>
public static class LogicalPlannerFactory
{
    private static readonly ITypeSource Source = new AssemblyTypeSource(typeof(LogicalPlannerFactory).Assembly);

    public static LogicalPlanner Create(FunctionCatalog? catalog = null)
        => new(new BuiltInPlanningContext(
            catalog ?? FunctionCatalog.Default,
            new AccumulatorRegistry(Source),
            new CoercionRegistry(Source),
            ExpressifTypeRegistry.Instance));

    private sealed class BuiltInPlanningContext(
        FunctionCatalog catalog,
        AccumulatorRegistry accumulators,
        CoercionRegistry coercions,
        ITypeRegistry types) : ILogicalPlanningContext
    {
        public PlannerFunctionMetadata? FindFunction(
            string name,
            string? expectedKind = null,
            int? argumentCount = null)
        {
            var function = expectedKind == "predicate"
                ? Find(name, "predicate", argumentCount) ?? Find(name, "function", argumentCount)
                : Find(name, "function", argumentCount) ?? Find(name, null, argumentCount);
            if (expectedKind == "accumulator"
                && (function is null || !accumulators.TryResolve(
                    new OperatorIdentity(function.Namespace, function.Name), out _)))
            {
                return null;
            }
            var implementationKind = function is not null && function.Incremental
                ? "accumulator"
                : function?.Kind;
            return function is null ? null : new PlannerFunctionMetadata(
                new PlannerFunctionDescriptor(
                    function.Name,
                    string.IsNullOrWhiteSpace(function.Input) ? "any" : function.Input,
                    string.IsNullOrWhiteSpace(function.Output) ? "any" : function.Output,
                    function.Traversal is null
                        ? null
                        : new PlannerTraversalDescriptor(function.Traversal.Source, function.Traversal.Selection),
                    function.Semantics is null
                        ? null
                        : new PlannerSemanticsDescriptor(
                            function.Semantics.Cardinality,
                            function.Semantics.Dependency,
                            function.Semantics.Ordering),
                    implementationKind!,
                    function.Schema is null
                        ? null
                        : new PlannerSchemaDescriptor(
                            function.Schema.Input,
                            function.Schema.Output,
                            function.Schema.Parameters?.ToDictionary(
                                parameter => parameter.Key,
                                parameter => new PlannerParameterSchemaDescriptor(
                                    parameter.Value.Input,
                                    parameter.Value.Output,
                                    parameter.Value.Combine),
                                StringComparer.Ordinal),
                            function.Schema.Intrinsic,
                            function.Schema.Nullability,
                            function.Schema.Classification,
                            function.Schema.DynamicReason,
                            function.Schema.NullableWhen),
                    function.Namespace),
                function.Parameters.Select(parameter => new PlannerParameterMetadata(
                    new PlannerParameterDescriptor(
                        parameter.Name,
                        parameter.TypeOrKind,
                        parameter.Optional,
                        parameter.Variadic,
                        parameter.MinimumCardinality,
                        parameter.Evaluation is null
                            ? null
                            : new PlannerEvaluationDescriptor(
                                parameter.Evaluation.Frequency,
                                parameter.Evaluation.Source,
                                parameter.Evaluation.Context)),
                    DescribeOmission(parameter.Omission))).ToArray());
        }

        private FunctionDocumentation? Find(string name, string? kind, int? argumentCount)
            => (kind, argumentCount) switch
            {
                (null, null) => catalog.Find(name) ?? catalog.Find(name.ToKebabCase()),
                (not null, null) => catalog.Find(name, kind) ?? catalog.Find(name.ToKebabCase(), kind),
                (null, not null) => catalog.Find(name, argumentCount.Value)
                    ?? catalog.Find(name.ToKebabCase(), argumentCount.Value),
                (not null, not null) => catalog.Find(name, kind, argumentCount.Value)
                    ?? catalog.Find(name.ToKebabCase(), kind, argumentCount.Value),
            };

        public PlannerFunctionMetadata? FindCoercion(string sourceType, string targetType)
        {
            if (!types.TryResolve(sourceType, out var source)
                || source.RuntimeType is null
                || !types.TryResolve(targetType, out var target)
                || target.RuntimeType is null
                || target.RuntimeType.IsAssignableFrom(source.RuntimeType))
            {
                return null;
            }

            var targetTypes = target.RuntimeType.IsValueType
                ? new[] { target.RuntimeType, typeof(Nullable<>).MakeGenericType(target.RuntimeType) }
                : [target.RuntimeType];
            var coercionName = targetTypes
                .Select(candidate => coercions.TryResolve(source.RuntimeType, candidate, out var name) ? name : null)
                .FirstOrDefault(name => name is not null);
            return coercionName is null ? null : FindFunction(coercionName, "function", 0);
        }

        public string? FindType(string name)
            => types.TryResolve(name, out var descriptor)
                ? descriptor.Name
                : null;

        private static PlannerOmissionDescriptor? DescribeOmission(ParameterOmissionDocumentation? omission)
            => omission is null
                ? null
                : new PlannerOmissionDescriptor(
                    omission.Mode switch
                    {
                        ParameterOmissionMode.Constant => PlannerOmissionMode.Constant,
                        ParameterOmissionMode.EmptyVariadic => PlannerOmissionMode.EmptyVariadic,
                        ParameterOmissionMode.Absent => PlannerOmissionMode.Absent,
                        ParameterOmissionMode.EnvironmentDerived => PlannerOmissionMode.EnvironmentDerived,
                        _ => throw new LogicalPlanningException($"Unsupported omission mode '{omission.Mode}'."),
                    },
                    omission.Value.ValueKind == System.Text.Json.JsonValueKind.Undefined
                        ? default
                        : omission.Value.Clone(),
                    omission.Source);
    }
}
