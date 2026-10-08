using Expressif.Functions;
using Expressif.Library.Array;
using Expressif.Library.Numeric;
using Expressif.Library.Record;
using Expressif.Library.Special;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Counting;
using Expressif.Library.Array.Sequencing;
using Expressif.Values;
using System.Collections;

namespace Expressif.Testing.Introspection;

[TestFixture]
public class FunctionContractsTest
{
    [Test]
    public void TextFunction_ExposesTypedContract()
    {
        IFunction<string?, string?> function = new Upper();

        Assert.That(function.Evaluate("abc"), Is.EqualTo("ABC"));
    }

    [Test]
    public void TextCountingFunction_ExposesTypedContract()
    {
        IFunction<string?, int?> function = new Length();

        Assert.That(function.Evaluate("abc"), Is.EqualTo(3));
    }

    [Test]
    public void TemporalFunction_ExposesTypedContract()
    {
        IFunction<DateTime?, int?> function = new YearOfEra();

        Assert.That(function.Evaluate(new DateTime(2026, 8, 23)), Is.EqualTo(2026));
    }

    [Test]
    public void ArrayFunction_ExposesTypedContract()
    {
        IFunction<IEnumerable, IEnumerable?> function = new Reverse();

        Assert.That(function.Evaluate(new[] { 1, 2, 3 }), Is.EqualTo(new object?[] { 3, 2, 1 }));
    }

    [Test]
    public void SpecialFunction_ExposesTypedContract()
    {
        IFunction<object?, string> function = new AnyToAny();

        Assert.That(function.Evaluate(42), Is.EqualTo("(any)"));
    }

    [Test]
    public void RecordFunction_ExposesTypedContract()
    {
        IFunction<object?, RecordValue> function = new Expressif.Library.Record.Record();

        Assert.That(function.Evaluate(null), Is.Empty);
    }
}
