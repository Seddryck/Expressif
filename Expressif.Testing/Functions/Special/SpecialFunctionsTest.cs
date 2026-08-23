using System.Reflection;
using Expressif.Functions;
using Expressif.Functions.Special;
using Expressif.Testing.Conformance;
using Expressif.Values.Special;

namespace Expressif.Testing.Functions.Special;

[TestFixture]
public class SpecialFunctionsTest
{
    [Conformance]
    public void Coalesce_Valid_Expressions(object? value, string[] expressions, object? expected)
    {
        var context = new Context();
        context.CurrentObject.Set(value);
        var function = new ExpressionFactory().Instantiate($"coalesce({string.Join(", ", expressions)})", context);

        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [Test]
    public void Coalesce_FirstExpressionReturnsValue_RemainingExpressionsAreNotEvaluated()
    {
        var evaluated = false;
        var function = new Coalesce([
            _ => "Alice",
            _ => { evaluated = true; return "Bob"; }
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(function.Evaluate(new object()), Is.EqualTo("Alice"));
            Assert.That(evaluated, Is.False);
        });
    }

    [Test]
    public void Coalesce_AllExpressionsReceiveSameInput()
    {
        var input = new object();
        var received = new List<object?>();
        var function = new Coalesce([
            value => { received.Add(value); return null; },
            value => { received.Add(value); return "fallback"; }
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(function.Evaluate(input), Is.EqualTo("fallback"));
            Assert.That(received, Is.EqualTo(new[] { input, input }));
        });
    }

    [Test]
    public void Coalesce_OneExpression_ThrowsBindingError()
        => Assert.That(
            () => new Coalesce([_ => null]),
            Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());

    [Test]
    public void Coalesce_OneParsedExpression_ThrowsBindingError()
        => Assert.That(
            () => new ExpressionFactory().Instantiate("coalesce(^.name)", new Context()),
            Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());

    [Test]
    public void Coalesce_SelectedValue_ContinuesThroughPipeline()
    {
        var value = new Dictionary<string, object?>
        {
            ["nickname"] = null,
            ["name"] = "Alice"
        };
        var context = new Context();
        context.CurrentObject.Set(value);
        var function = new ExpressionFactory().Instantiate(
            "coalesce(^.nickname, ^.name, \"Anonymous\") | upper",
            context);

        Assert.That(function.Evaluate(value), Is.EqualTo("ALICE"));
    }

    [Test]
    public void Coalesce_NestedExpressions_EvaluatesEachArgument()
    {
        var value = new Dictionary<string, object?>
        {
            ["nickname"] = null,
            ["name"] = "Alice"
        };
        var context = new Context();
        context.CurrentObject.Set(value);
        var function = new ExpressionFactory().Instantiate(
            "coalesce(field(nickname), .name)",
            context);

        Assert.That(function.Evaluate(value), Is.EqualTo("Alice"));
    }

    [TestCase("coalesce(.nickname, .name)", "Alice")]
    [TestCase("coalesce(field(nickname), field(name))", "Alice")]
    [TestCase("coalesce(field(\"nickname\"), field(\"name\"))", "Alice")]
    [TestCase("coalesce(.nickname, field(name))", "Alice")]
    [TestCase("coalesce(.nickname | upper, .name | upper)", "ALICE")]
    [TestCase("coalesce(.nickname, .display-name, .name)", "Alice")]
    [TestCase("coalesce(.nickname, .display-name)", null)]
    public void Coalesce_MissingField_ContinuesWithNextCandidate(string expression, object? expected)
    {
        var value = new Dictionary<string, object?> { ["name"] = "Alice" };
        var context = new Context();
        context.CurrentObject.Set(value);
        var function = new ExpressionFactory().Instantiate(expression, context);

        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [Test]
    public void Coalesce_ExplicitNullField_ContinuesWithNextCandidate()
    {
        var value = new Dictionary<string, object?>
        {
            ["nickname"] = null,
            ["name"] = "Alice"
        };
        var context = new Context();
        context.CurrentObject.Set(value);
        var function = new ExpressionFactory().Instantiate("coalesce(.nickname, .name)", context);

        Assert.That(function.Evaluate(value), Is.EqualTo("Alice"));
    }

    [Test]
    [TestCase("foo")]
    [TestCase("(any)")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    [TestCase("(value)")]
    [TestCase("(null)")]
    [TestCase(null)]
    [TestCase(150)]
    public void AnyToAny_Any(object? value)
        => Assert.That(new AnyToAny().Evaluate(value), Is.EqualTo(new Any()));

    [Test]
    [TestCase(typeof(Null))]
    [TestCase(typeof(Empty))]
    [TestCase(typeof(Whitespace))]
    [TestCase(typeof(Any))]
    [TestCase(typeof(Value))]
    public void AnyToAny_SpecialType_Any(Type type)
        => Assert.That(new AnyToAny().Evaluate(
            type.GetConstructor([])!.Invoke(System.Array.Empty<Type>()))
            , Is.EqualTo(new Any()));

    [Test]
    [TestCase(typeof(DBNull))]
    public void AnyToAny_DBNull_Any(Type type)
        => Assert.That(new AnyToAny().Evaluate(
            type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
            , Is.EqualTo(new Any()));

    [Test]
    [TestCase("foo")]
    [TestCase("(any)")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    [TestCase("(value)")]
    [TestCase(150)]
    public void ValueToValue_NotNull_Value(object value)
        => Assert.That(new ValueToValue().Evaluate(value), Is.EqualTo(new Value()));

    [Test]
    [TestCase(typeof(Empty))]
    [TestCase(typeof(Whitespace))]
    [TestCase(typeof(Any))]
    [TestCase(typeof(Value))]
    public void ValueToValue_SpecialType_Value(Type type)
        => Assert.That(new ValueToValue().Evaluate(
            type.GetConstructor([])!.Invoke(System.Array.Empty<Type>()))
            , Is.EqualTo(new Value()));

    [Test]
    [TestCase("(null)")]
    [TestCase(null)]
    public void ValueToValue_Null_Null(object? value)
        => Assert.That(new ValueToValue().Evaluate(value), Is.EqualTo(new Null()));

    [Test]
    [TestCase(typeof(Null))]
    public void ValueToValue_SpecialType_Null(Type type)
        => Assert.That(new ValueToValue().Evaluate(
            type.GetConstructor([])!.Invoke(System.Array.Empty<Type>()))
            , Is.EqualTo(new Null()));

    [Test]
    [TestCase(typeof(DBNull))]
    public void ValueToValue_DBNull_Null(Type type)
        => Assert.That(new ValueToValue().Evaluate(
            type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
            , Is.EqualTo(new Null()));

    [Test]
    [TestCase("(null)")]
    [TestCase(null)]
    public void NullToValue_Null_Value(object? value)
        => Assert.That(new NullToValue().Evaluate(value), Is.EqualTo(new Value()));

    [Test]
    [TestCase(typeof(Null))]
    public void NullToValue_SpecialType_Null(Type type)
        => Assert.That(new NullToValue().Evaluate(
            type.GetConstructor([])!.Invoke(System.Array.Empty<Type>()))
            , Is.EqualTo(new Value()));

    [Test]
    [TestCase(typeof(DBNull))]
    public void NullToValue_DBNull_Null(Type type)
        => Assert.That(new NullToValue().Evaluate(
            type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
            , Is.EqualTo(new Value()));

    [Conformance]
    public void NullToValue_Value_NotNull(object value, object expected)
        => Assert.That(new NullToValue().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(typeof(Empty))]
    [TestCase(typeof(Whitespace))]
    [TestCase(typeof(Any))]
    [TestCase(typeof(Value))]
    public void NullToValue_SpecialType_Value(Type type)
    {
        var obj = type.GetConstructor([])!.Invoke(System.Array.Empty<Type>());
        Assert.That(new NullToValue().Evaluate(obj), Is.EqualTo(obj));
    }
}
