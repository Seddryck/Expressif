using Expressif.Bindings;
using Expressif.Library.Catalog;
using Expressif.Planning;
using Expressif.Syntax;
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
        public IRootExpression Bind(RootExpressionSyntax syntax)
            => ExpressifBinderFactory.Create(applyCoercion: false).Bind(syntax);

        public PlannerFunctionMetadata? FindFunction(string name)
        {
            var function = catalog.Find(name);
            return function is null ? null : new PlannerFunctionMetadata(
                new PlannerFunctionDescriptor(
                    function.Name,
                    function.Input,
                    function.Output,
                    function.Traversal is null
                        ? null
                        : new PlannerTraversalDescriptor(function.Traversal.Source, function.Traversal.Selection),
                    function.Semantics is null
                        ? null
                        : new PlannerSemanticsDescriptor(
                            function.Semantics.Cardinality,
                            function.Semantics.Dependency,
                            function.Semantics.Ordering)),
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

        public string? FindTypeName(Type runtimeType)
            => ExpressifTypeRegistry.Instance.All
                .SingleOrDefault(candidate => candidate.RuntimeType == runtimeType)?.Name;

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
