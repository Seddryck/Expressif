using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class AnyOfTest
{
    [Conformance]
    public void AnyOf_Valid_Text(object value, string[] references, bool expected)
    {
        var scalars = new Func<List<string>>(() => references.ToList());
        var predicate = new AnyOf(scalars);
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
    }
}
