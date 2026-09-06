using Expressif.Functions;
using Expressif.Functions.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Record;

public class VisibilityFunctionsTest
{
    [Conformance]
    public void Public_Valid_Record(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Test]
    public void Public_TypedProjection_PreservesInputAndValueIdentity()
    {
        var nested = new RecordValue();
        nested.Set("_secret", 1);
        var input = new RecordValue();
        input.Set("nested", nested);
        input.Set("_private", 2);
        IFunction<RecordValue, RecordValue> function = new Public();
        var result = function.Evaluate(input);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(input));
            Assert.That(result["nested"], Is.SameAs(nested));
            Assert.That(input.ContainsKey("_private"), Is.True);
        });
    }
}
