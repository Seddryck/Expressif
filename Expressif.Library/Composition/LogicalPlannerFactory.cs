using Expressif.Library.Catalog;
using Expressif.Planning;
using Expressif.Values.Types;

namespace Expressif.Library.Composition;

/// <summary>
/// Composes Core logical planning with the official built-in Expressif vocabulary.
/// </summary>
public static class LogicalPlannerFactory
{
    public static LogicalPlanner Create(FunctionCatalog? catalog = null)
        => new(new BuiltInPlanningContext(catalog ?? FunctionCatalog.Default));

    private sealed class BuiltInPlanningContext(FunctionCatalog catalog) : ILogicalPlanningContext
    {
        public PlannerFunctionMetadata? FindFunction(string name, string? expectedKind = null)
        {
            var function = expectedKind is "predicate" or "accumulator"
                ? Find(name, expectedKind) ?? Find(name, "function")
                : Find(name, "function") ?? Find(name);
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
                    function.Kind,
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
                            function.Schema.NullableWhen)),
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

        private FunctionDocumentation? Find(string name, string? kind = null)
            => kind is null
                ? catalog.Find(name) ?? catalog.Find(name.ToKebabCase())
                : catalog.Find(name, kind) ?? catalog.Find(name.ToKebabCase(), kind);

        public string? FindType(string name)
            => ExpressifTypeRegistry.Instance.TryResolve(name, out var descriptor)
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
