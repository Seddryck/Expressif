using System.Text.Json;
using Expressif.Cli.Commands;
using Expressif.Planning;

namespace Expressif.Cli.Tests;

public class LogicalPlanFormatterTests
{
    [Test]
    public void Format_Arguments_RendersExplicitSpreadAndOmittedShapes()
    {
        using var omissionValue = JsonDocument.Parse("1");
        var parameter = new PlannerParameterDescriptor("value", "any", true, true, 0);
        var call = new LogicalCall(
            new PlannerFunctionDescriptor("sample", "any", "any"),
            [
                new LogicalArgument(parameter, new LogicalLiteral("text", "plain"), false, true),
                new LogicalArgument(parameter, new LogicalLiteral("integer", 1), true, true),
                new LogicalArgument(parameter, null, false, false, new PlannerOmissionDescriptor(
                    PlannerOmissionMode.Constant,
                    omissionValue.RootElement.Clone())),
                new LogicalArgument(parameter, null, false, false),
            ]);

        var result = LogicalPlanFormatter.Format(new LogicalPlan(new LogicalPipeline([call])));

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("├─ Argument: value"));
            Assert.That(result, Does.Contain("Literal: text = \"plain\""));
            Assert.That(result, Does.Contain("Argument: value (spread)"));
            Assert.That(result, Does.Contain("Literal: integer = 1"));
            Assert.That(result, Does.Contain("Argument: value (omitted: Constant)"));
            Assert.That(result, Does.Contain("Argument: value (omitted: unspecified)"));
        });
    }
}
