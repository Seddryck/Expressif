using Expressif.Functions;
using Expressif.Functions.Array;
using Expressif.Predicates;
using Expressif.Testing.Conformance;
using System.Globalization;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class GenerateTest
{
    [Conformance]
    public void Generate_Valid_While_Next_OptionalResult(object? input, string[] parameters, decimal[] expected)
    {
        var seed = input is string text && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeric)
            ? numeric
            : input;
        var result = parameters.Length == 2
            ? new ExpressionFactory().Create($"generate(while := {parameters[0]}, next := {parameters[1]})").Evaluate(seed)
            : new ExpressionFactory().Create($"generate(while := {parameters[0]}, next := {parameters[1]}, result := {parameters[2]})").Evaluate(seed);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Conformance]
    public void Generate_Valid_Temporal_DateSequence(DateOnly input, string[] parameters, DateOnly[] expected)
        => Assert.That(Evaluate(input, parameters), Is.EqualTo(expected));

    [Conformance]
    public void Generate_Valid_Temporal_TimeSequence(DateOnly input, string[] parameters, TimeOnly[] expected)
        => Assert.That(Evaluate(input, parameters), Is.EqualTo(expected));

    [Test]
    public void Evaluate_UsesCurrentSeedForResultAndNextInThatOrder()
    {
        var calls = new List<string>();
        var generate = new Generate(
            () => new DelegatedPredicate(value => { calls.Add($"while:{value}"); return (int)value! <= 2; }),
            () => new DelegatedFunction(value => { calls.Add($"next:{value}"); return (int)value! + 1; }),
            () => new DelegatedFunction(value => { calls.Add($"result:{value}"); return (int)value! * 10; }));

        Assert.That(generate.Evaluate(1), Is.EqualTo(new object?[] { 10, 20 }));
        Assert.That(calls, Is.EqualTo(new[] { "while:1", "result:1", "next:1", "while:2", "result:2", "next:2", "while:3" }));
    }

    [Test]
    public void Evaluate_NullSeedCanProduceEmptyArray()
        => Assert.That(
            new Generate(() => new DelegatedPredicate(value => value is not null), () => new DelegatedFunction(value => value)).Evaluate(null),
            Is.EqualTo(System.Array.Empty<object?>()));

    [Test]
    public void Instantiate_ResultMayPrecedeNext()
    {
        var generate = new ExpressionFactory().Create(
            "generate(while := less-than(3), result := multiply(10), next := add(1))");

        Assert.That(generate.Evaluate(1), Is.EqualTo(new object?[] { 10m, 20m }));
    }

    private static object? Evaluate(object input, string[] parameters)
        => new ExpressionFactory().Create(
            $"generate(while := {parameters[0]}, next := {parameters[1]}, result := {parameters[2]})")
            .Evaluate(input);

    private sealed class DelegatedFunction(Func<object?, object?> implementation) : IFunction
    {
        public object? Evaluate(object? value) => implementation(value);
    }

    private sealed class DelegatedPredicate(Func<object?, bool> implementation) : IPredicate
    {
        public bool Evaluate(object? value) => implementation(value);
        object? IFunction.Evaluate(object? value) => Evaluate(value);
    }
}
