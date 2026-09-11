using Expressif.Functions.Flow;
using Expressif.Functions.Introspection;
using Expressif.Predicates;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Flow;

public class ThrowTest
{
    [Conformance]
    public void Throw_Accepted(object? value, string predicate, string? expected)
        => Assert.That(Expression.Create($"throw({predicate}) | upper").Evaluate(value), Is.EqualTo(expected));

    [TestCase("#null | throw() | upper")]
    [TestCase("-1 | throw(is-negative) | absolute")]
    [TestCase("1 | throw(predicate:= is-positive)")]
    [TestCase("{rejected := #false, item := {rejected := #true}} | .item | throw(.rejected)")]
    [TestCase("#null | catch(throw(#true))")]
    [TestCase("1 | throw(throw(#true))")]
    public void Evaluate_Rejected(string source)
        => Assert.Throws<EvaluationException>(() => Expression.Create(source).Evaluate(null));

    [TestCase("12.346 | throw(is-negative) | round(2)", 12.35)]
    [TestCase("#null | throw(#false)", null)]
    [TestCase("{rejected := #true, item := {rejected := #false}} | .item | throw(.rejected) | .rejected", false)]
    public void Evaluate_AcceptedContinues(string source, object? expected)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(expected));

    [Test]
    public void Evaluate_NullSemantics()
        => Assert.Throws<EvaluationException>(() => new Throw().Evaluate(DBNull.Value));

    [Test]
    public void Evaluate_PredicateOnceAndErrorsPropagate()
    {
        var predicate = new CountingPredicate();
        var function = new Throw(() => predicate);
        Assert.That(function.Evaluate(42), Is.EqualTo(42));
        Assert.That(predicate.Count, Is.EqualTo(1));
        predicate.Fail = true;
        Assert.Throws<InvalidOperationException>(() => function.Evaluate(42));
        Assert.That(predicate.Count, Is.EqualTo(2));
    }

    [TestCase("throw(1, 2)")]
    [TestCase("throw(other:= is-null)")]
    public void Bind_InvalidArguments(string source)
        => Assert.Catch<ExpressifException>(() => Expression.Create(source));

    [Test]
    public void Evaluate_RequiresBoolean()
        => Assert.Throws<InvalidCastException>(() => Expression.Create("throw(42)").Evaluate(1));

    [TestCase("catch", false, "expression")]
    [TestCase("throw", true, "predicate")]
    public void Describe_DynamicContract(string name, bool optional, string parameterType)
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == name);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Input, Is.EqualTo("any"));
            Assert.That(info.Output, Is.EqualTo("any"));
            Assert.That(info.Converted, Is.False);
            Assert.That(info.Reason, Does.Contain("input type"));
            Assert.That(info.Scope, Is.EqualTo("flow"));
            Assert.That(info.Parameters.Single().Optional, Is.EqualTo(optional));
            Assert.That(info.Parameters.Single().Type, Is.EqualTo(parameterType));
        }
    }

    private sealed class CountingPredicate : BasePredicate
    {
        public int Count { get; private set; }
        public bool Fail { get; set; }

        public override bool Evaluate(object? value)
        {
            Count++;
            return Fail ? throw new InvalidOperationException() : false;
        }
    }
}
