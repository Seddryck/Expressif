using System.Collections;
using Expressif.Functions;
using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class ToTupleTest
{
    [Conformance]
    public void ToTuple_Valid(object input, string? expected)
    {
        var value = input is string text && text.StartsWith('{')
            ? new ParameterValueConverter().Parse(text)
            : input;
        var actual = new ToTuple().Evaluate(value);

        Assert.That(actual is null ? null : ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void ToTuple_ExposesTypedArrayToTupleContract()
    {
        IFunction<IEnumerable, TupleValue?> function = new ToTuple();

        Assert.That(function.Evaluate(new object?[] { 1, "A", true }),
            Is.EqualTo(new TupleValue(1, "A", true)));
    }

    [Test]
    public void ToTuple_PreservesNestedValuesWithoutRecursiveConversion()
    {
        var nestedArray = new object?[] { 2, 3 };
        var nestedRecord = new RecordValue();
        nestedRecord.Set("name", "Bob");
        var nestedTuple = new TupleValue(4, 5);
        var source = new object?[] { 1, null, nestedArray, nestedRecord, nestedTuple };

        var actual = (TupleValue)new ToTuple().Evaluate(source)!;

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(source.Length));
            Assert.That(actual[0], Is.EqualTo(1));
            Assert.That(actual[1], Is.Null);
            Assert.That(actual[2], Is.SameAs(nestedArray));
            Assert.That(actual[3], Is.SameAs(nestedRecord));
            Assert.That(actual[4], Is.SameAs(nestedTuple));
        });
    }
}
