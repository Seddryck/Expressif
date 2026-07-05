using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Values;
using Expressif.Values.Special;
using System.Data;
using System.Diagnostics;

namespace Expressif.Testing;

public class ExpressionTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Evaluate_SingleFunctionWithoutParameter_Valid()
    {
        var expression = new Expression("lower");
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola tesla"));
    }

    [Test]
    public void Evaluate_SingleFunctionWithOneParameter_Valid()
    {
        var expression = new Expression("remove-chars(a)");
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("Nikol Tesl"));
    }

    [Test]
    public void Evaluate_TwoFunctions_Valid()
    {
        var expression = new Expression("lower | remove-chars(a)");
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikol tesl"));
    }

    [Test]
    public void Evaluate_VariableAsParameter_Valid()
    {
        var context = new Context();
        context.Variables.Add<char>("myChar", 'k');

        var expression = new Expression("lower | remove-chars(@myChar)", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("niola tesla"));
    }

    [Test]
    public void Evaluate_VariableAsParameterDoublePass_Valid()
    {
        var context = new Context();
        var expression = new Expression("lower | remove-chars(@myChar)", context);

        context.Variables.Add<char>("myChar", 'k');
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("niola tesla"));
        context.Variables.Set("myChar", 'a');
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol tesl"));
    }

    [Test]
    public void Evaluate_ObjectPropertyAsParameter_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new { CharToBeRemoved = 't' });

        var expression = new Expression("lower | remove-chars([CharToBeRemoved])", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola esla"));
    }

    [Test]
    public void Evaluate_ObjectPropertyAsParameterDoublePass_Valid()
    {
        var context = new Context();
        var expression = new Expression("lower | remove-chars([CharToBeRemoved])", context);

        context.CurrentObject.Set(new { CharToBeRemoved = 't' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola esla"));

        context.CurrentObject.Set(new { CharToBeRemoved = 'k' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("niola tesla"));
    }

    [Test]
    public void Evaluate_ObjectIndexAsParameter_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<char>() { 'e', 's' });

        var expression = new Expression("lower | remove-chars(#1)", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola tela"));
    }

    [Test]
    public void Evaluate_ObjectIndexAsParameterDoublePass_Valid()
    {
        var context = new Context();
        var expression = new Expression("lower | remove-chars(#1)", context);

        context.CurrentObject.Set(new List<char>() { 'e', 's' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tela"));

        context.CurrentObject.Set(new List<char>() { 'e', 'o' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikla tesla"));
    }

    [Test]
    public void Evaluate_AliasesPrefix_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<char>() { 'e', 's' });

        var expression = new Expression("text-to-lower | text-to-remove-chars(#1)", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola tela"));
    }

    [Test]
    public void Evaluate_AliasesDateTime_Valid()
    {
        var expression = new Expression("dateTime-to-add(04:00:00, 4)", new Context());
        var result = expression.Evaluate("2023-12-28 02:00:00");
        Assert.That(result, Is.EqualTo(DateTime.Parse("2023-12-28 18:00:00")));
    }

    [Test]
    public void Evaluate_CalendarCatholic_Valid()
    {
        var expression = new Expression("calendar-catholic(\"eAsTeR sUnDaY\")", new Context());
        var result = expression.Evaluate(2023);
        Assert.That(result, Is.EqualTo(DateTime.Parse("2023-04-09")));
    }

    [Test]
    public void Evaluate_CalendarCatholicWithKind_Valid()
    {
        var expression = new Expression("calendar-catholic(\"The Assumption\", \"Utc\")", new Context());
        var result = (DateTime)expression.Evaluate(2023)!;
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(DateTime.SpecifyKind(DateTime.Parse("2023-08-15"), DateTimeKind.Utc)));
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    [TestCase("null-to-empty | count-chars")]
    [TestCase("null-to-empty | text-to-length")]
    [TestCase("null-to-empty | length")]
    public void Evaluate_AliasesAllStyles_Valid(string code)
    {
        var expression = new Expression(code, new Context());
        var result = expression.Evaluate("foo");
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void Evaluate_FunctionAsParameter_Valid()
    {
        var context = new Context();
        context.Variables.Add<int>("myVar", 6);
        context.CurrentObject.Set(new List<decimal>() { 15, 8, 3 });

        var expression = new Expression("lower | skip-last-chars( {@myVar | subtract(#2) })", context);
        var result = expression.Evaluate("Nikola Tesla");
        Assert.That(result, Is.EqualTo("nikola te"));
    }

    [Test]
    public void Evaluate_FunctionWithIntegerForDecimal_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<int>() { 2, 3 });

        var expression = new Expression("numeric-to-multiply(#1)", context);
        var result = expression.Evaluate(10);
        Assert.That(result, Is.EqualTo(30));
    }

    [Test]
    public void Evaluate_FunctionWithTextForDateTime_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<string>() { "2020-01-01", "2021-12-31" });

        var expression = new Expression("dateTime-to-clip(#0, #1)", context);
        var result = expression.Evaluate("2018-01-01");
        Assert.That(result, Is.EqualTo(new DateTime(2020, 01, 01)));
    }

    [Test]
    public void Evaluate_ArrayLiteralPipeSum_Valid()
    {
        var expression = new ClosedExpression("{1,2,3} | sum");
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(6m));
    }

    [Test]
    public void Evaluate_VariableArrayPipeCount_Valid()
    {
        var context = new Context();
        context.Variables.Add<int[]>("arr", new[] { 1, 2, 3, 4 });

        var expression = new ClosedExpression("@arr | count", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_ObjectPropertyArrayPipeSum_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new { Values = new object[] { "1", 2, true } });

        var expression = new ClosedExpression("[Values] | sum", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(4m));
    }

    [Test]
    public void Evaluate_ObjectIndexArrayPipeMax_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<object> { new[] { 1, 9, 4 } });

        var expression = new ClosedExpression("#0 | max", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(9m));
    }

    [Test]
    public void Evaluate_OpenExpression_StartWithArrayFunctionThenAdd_Valid()
    {
        var expression = new Expression("sum | add(4)");
        var result = expression.Evaluate(new[] { 1, 2, 3 });
        Assert.That(result, Is.EqualTo(10m));
    }

    [Test]
    public void Evaluate_DirectAccumulatorSyntax_StillScalar()
    {
        var expression = new ClosedExpression("{1,2,3} | sum");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(6m));
    }

    [Test]
    public void Evaluate_ArrayPipeMapMultiply_Valid()
    {
        var expression = new ClosedExpression("{1,2,3} | map(multiply(2))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 2m, 4m, 6m }));
    }

    [Test]
    public void Evaluate_ArrayPipeMapAdd_Valid()
    {
        var expression = new ClosedExpression("{1,2,3} | map(add(10))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { 11m, 12m, 13m }));
    }

    [Test]
    public void Evaluate_StringArrayPipeMapUpper_Valid()
    {
        var expression = new ClosedExpression("{\"alice\",\"bob\"} | map(upper)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { "ALICE", "BOB" }));
    }

    [Test]
    public void Evaluate_Map_PreservesCardinality()
    {
        var expression = new ClosedExpression("{1,2,3,4} | map(add(1))");
        var result = expression.Evaluate() as object?[];

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Length.EqualTo(4));
    }

    [Test]
    public void Evaluate_EmptyArrayPipeMap_EmptyArray()
    {
        var expression = new ClosedExpression("{} | map(add(1))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(Array.Empty<object?>()));
    }

    [Test]
    public void Evaluate_ArrayPipeFilterGreaterThan_Valid()
    {
        var expression = new ClosedExpression("{1,2,3,4} | filter(greater-than(2))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { "3", "4" }));
    }

    [Test]
    public void Evaluate_ArrayPipeFilterEven_Valid()
    {
        var expression = new ClosedExpression("{1,2,3,4,5} | filter(even)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { "2", "4" }));
    }

    [Test]
    public void Evaluate_StringArrayPipeFilterStartsWith_Valid()
    {
        var expression = new ClosedExpression("{\"alice\",\"bob\",\"anna\"} | filter(starts-with(a))");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(new object?[] { "alice", "anna" }));
    }

    [Test]
    public void Evaluate_EmptyArrayPipeFilter_EmptyArray()
    {
        var expression = new ClosedExpression("{} | filter(even)");
        var result = expression.Evaluate();

        Assert.That(result, Is.EqualTo(Array.Empty<object?>()));
    }

    [Test]
    public void Evaluate_FilterWithNonPredicateExpression_Throws()
    {
        var expression = new ClosedExpression("{1,2,3} | filter(add(1))");

        Assert.That(() => expression.Evaluate(), Throws.TypeOf<NotImplementedFunctionException>());
    }

    [Test]
    public void Evaluate_EmptyArrayPipeAggregators_Valid()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new ClosedExpression("{} | count").Evaluate(), Is.Zero);
            Assert.That(new ClosedExpression("{} | sum").Evaluate(), Is.Zero);
            Assert.That(new ClosedExpression("{} | min").Evaluate(), Is.Null);
            Assert.That(new ClosedExpression("{} | max").Evaluate(), Is.Null);
            Assert.That(new ClosedExpression("{} | first").Evaluate(), Is.Null);
            Assert.That(new ClosedExpression("{} | last").Evaluate(), Is.Null);
        }
    }
}
