using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Grouping;

public class DrillDownTest
{
    [Test]
    public void Evaluate_VisitsCurrentValuesInOrderAndResetsBetweenCalls()
    {
        var visited = new List<string>();
        var function = new Expressif.Functions.Grouping.DrillDown([
            value => { visited.Add($"first:{value}"); return value; },
            value => { visited.Add($"second:{value}"); return null; },
        ]);
        var input = new Expressif.Values.Grouping([new PairValue("key", new[] { 2, 1, 2 })]);

        var first = function.Evaluate(input);
        var second = function.Evaluate(input);

        Assert.Multiple(() =>
        {
            Assert.That(visited, Is.EqualTo(Enumerable.Repeat(new[] { "first:2", "second:2", "first:1", "second:1", "first:2", "second:2" }, 2).SelectMany(items => items)));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(input[0].Values, Is.EqualTo(new[] { 2, 1, 2 }));
        });
    }

    [TestCase("#{} | drill-down()", typeof(MissingOrUnexpectedParametersFunctionException))]
    [TestCase("#{} | drill-down(expressions := lower)", typeof(MissingOrUnexpectedParametersFunctionException))]
    [TestCase("#{} | drill-down(...{1, 2})", typeof(Expressif.Bindings.BindingException))]
    public void Create_RejectsUnsupportedArguments(string expression, Type exceptionType)
        => Assert.That(() => Expression.Create(expression), Throws.TypeOf(exceptionType));

    [Conformance]
    public void DrillDown_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));
}
