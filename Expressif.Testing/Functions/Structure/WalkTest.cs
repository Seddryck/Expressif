using Expressif.Functions;
using Expressif.Functions.Structure;
using Expressif.Values;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Structure;

public class WalkTest
{
    [Conformance]
    public void Walk_Array(object?[] value, string expression, object?[] expected)
        => Assert.That(Expression.Create($"walk({expression})").Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Evaluate_Array_TransformsLeavesRecursively()
    {
        var input = new object?[] { " Nikola ", new object?[] { " Tesla ", " Edison " } };
        Assert.That(Expression.Create("walk(*trim)").Evaluate(input),
            Is.EqualTo(new object?[] { "Nikola", new object?[] { "Tesla", "Edison" } }));
    }

    [Test]
    public void Evaluate_Tuple_PreservesTupleStructure()
        => Assert.That(Expression.Create("walk(trim)").Evaluate(new TupleValue(42, " 42 ")),
            Is.EqualTo(new TupleValue("42", "42")));

    [Test]
    public void Evaluate_GuardedTuple_PreservesIncompatibleLeafType()
        => Assert.That(Expression.Create("walk(*trim)").Evaluate(new TupleValue(42, " 42 ")),
            Is.EqualTo(new TupleValue(42, "42")));

    [Test]
    public void Evaluate_Record_TransformsValuesAndPreservesFieldNames()
    {
        var input = new RecordValue();
        input.Set(" name ", " Nikola ");
        input.Set("age", 42);

        var result = (RecordValue)Expression.Create("walk(*trim)").Evaluate(input)!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Keys, Is.EqualTo(new[] { " name ", "age" }));
            Assert.That(result[" name "], Is.EqualTo("Nikola"));
            Assert.That(result["age"], Is.EqualTo(42));
        }
    }

    [Test]
    public void Evaluate_NestedRecords_TransformsLeavesAndPreservesEveryFieldName()
    {
        var address = new RecordValue();
        address.Set(" city ", " Brussels ");
        address.Set("postal-code", 1000);
        var customer = new RecordValue();
        customer.Set(" name ", " Nikola ");
        customer.Set("address", address);

        var result = (RecordValue)Expression.Create("walk(*trim)").Evaluate(customer)!;
        var nested = (RecordValue)result["address"]!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Keys, Is.EqualTo(new[] { " name ", "address" }));
            Assert.That(result[" name "], Is.EqualTo("Nikola"));
            Assert.That(nested.Keys, Is.EqualTo(new[] { " city ", "postal-code" }));
            Assert.That(nested[" city "], Is.EqualTo("Brussels"));
            Assert.That(nested["postal-code"], Is.EqualTo(1000));
        }
    }

    [Test]
    public void Evaluate_ExpressionReturningNull_ReplacesLeaf()
        => Assert.That(new Walk(() => new DelegatedFunction(_ => null)).Evaluate(new object?[] { 1 }),
            Is.EqualTo(new object?[] { null }));

    [Test]
    public void Evaluate_GuardedPipeline_AppliesWholeExpressionOnlyToCompatibleLeaves()
        => Assert.That(Expression.Create("walk(*(trim | append-space))").Evaluate(new TupleValue(42, " Bob ")),
            Is.EqualTo(new TupleValue(42, "Bob ")));

    private sealed class DelegatedFunction(Func<object?, object?> evaluate) : IFunction
    {
        public object? Evaluate(object? value) => evaluate(value);
    }
}
