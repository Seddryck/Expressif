using System.Text.Json;
using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Testing.Planning;

public class CoreLogicalPlannerTest
{
    [Test]
    public void Build_CustomVocabulary_UsesCanonicalNameAndPreservesOmission()
    {
        var omissionValue = JsonDocument.Parse("1").RootElement.Clone();
        var metadata = new PlannerFunctionMetadata(
            new PlannerFunctionDescriptor("add", "numeric", "numeric"),
            [
                new PlannerParameterMetadata(new PlannerParameterDescriptor("value", "numeric", false, false, 1)),
                new PlannerParameterMetadata(
                    new PlannerParameterDescriptor("times", "numeric", true, false, 1),
                    new PlannerOmissionDescriptor(PlannerOmissionMode.Constant, omissionValue)),
            ]);
        var plan = new LogicalPlanner(new StubContext(metadata)).Build(ExpressionParser.Parse("custom-alias(5)"));
        var call = (LogicalCall)plan.Pipeline.Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo("add"));
            Assert.That(call.Arguments[0].Parameter.Name, Is.EqualTo("value"));
            Assert.That(call.Arguments[1].IsExplicit, Is.False);
            Assert.That(call.Arguments[1].Omission?.Value.GetDecimal(), Is.EqualTo(1m));
        });
    }

    [Test]
    public void Build_CustomVocabulary_InsertsResolvedPipelineCoercion()
    {
        var plan = new LogicalPlanner(new CoercionContext())
            .Build(ExpressionParser.Parse("1 | custom-upper | custom-add"));

        Assert.That(plan.Pipeline.Items.OfType<LogicalCall>().Select(call => call.Function.Name),
            Is.EqualTo(new[] { "coerce-text", "custom-upper", "coerce-numeric", "custom-add" }));
    }

    [Test]
    public void PlanningTypesAndSchema_AreOwnedByCore()
    {
        var assembly = typeof(LogicalPlanner).Assembly;

        Assert.Multiple(() =>
        {
            Assert.That(typeof(LogicalPlan).Assembly, Is.SameAs(assembly));
            Assert.That(typeof(LogicalPlanJson).Assembly, Is.SameAs(assembly));
            Assert.That(assembly.GetManifestResourceNames(), Does.Contain(LogicalPlanJson.SchemaResourceName));
            Assert.That(assembly.GetReferencedAssemblies().Select(reference => reference.Name),
                Does.Not.Contain("Expressif.Library"));
            Assert.That(typeof(ILogicalPlanningContext).GetMethods()
                    .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
                        .Append(method.ReturnType))
                    .Select(type => type.Namespace),
                Does.Not.Contain("Expressif.Bindings"));
        });
    }

    private sealed class StubContext(PlannerFunctionMetadata metadata) : ILogicalPlanningContext
    {
        public PlannerFunctionMetadata? FindFunction(
            string name,
            string? expectedKind = null,
            int? argumentCount = null)
            => name == "custom-alias" ? metadata : null;

        public string? FindType(string name) => null;
    }

    private sealed class CoercionContext : ILogicalPlanningContext
    {
        private static readonly PlannerFunctionMetadata Upper = Function("custom-upper", "text", "text");
        private static readonly PlannerFunctionMetadata Add = Function("custom-add", "numeric", "numeric");
        private static readonly PlannerFunctionMetadata ToText = Function("coerce-text", "any", "text");
        private static readonly PlannerFunctionMetadata ToNumeric = Function("coerce-numeric", "any", "numeric");

        public PlannerFunctionMetadata? FindFunction(
            string name,
            string? expectedKind = null,
            int? argumentCount = null)
            => name switch
            {
                "custom-upper" => Upper,
                "custom-add" => Add,
                _ => null,
            };

        public PlannerFunctionMetadata? FindCoercion(string sourceType, string targetType)
            => (sourceType, targetType) switch
            {
                ("decimal", "text") => ToText,
                ("text", "numeric") => ToNumeric,
                _ => null,
            };

        public string? FindType(string name) => null;

        private static PlannerFunctionMetadata Function(string name, string input, string output)
            => new(new PlannerFunctionDescriptor(name, input, output), []);
    }
}
