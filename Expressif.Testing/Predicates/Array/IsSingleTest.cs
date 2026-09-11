using System.Collections;
using Expressif.Functions;
using Expressif.Predicates.Array;
using Expressif.Predicates.Introspection;
using Expressif.Testing.Conformance;
using SingleFunction = Expressif.Functions.Array.Single;

namespace Expressif.Testing.Predicates.Array;

[TestFixture]
public class IsSingleTest
{
    [Conformance]
    public void IsSingle_Valid_Cardinality(object? input, bool expected)
    {
        IFunction<IEnumerable, bool> predicate = new IsSingle();
        Assert.That(predicate.Evaluate((IEnumerable)input!), Is.EqualTo(expected));
        if (expected)
            Assert.That(new SingleFunction().Evaluate(input), Is.EqualTo(((IEnumerable)input!).Cast<object?>().Single()));
    }

    [Conformance]
    public void IsSingle_Valid_Conversion(object? input, bool expected)
        => Assert.That(new IsSingle().Evaluate(input), Is.EqualTo(expected));

    [TestCase("{42} | is-single", true)]
    [TestCase("{} | is-single", false)]
    [TestCase("{1, 2} | is-single", false)]
    [TestCase("{#null} | is-single", true)]
    [TestCase("42 | is-single", false)]
    [TestCase("#null | is-single", false)]
    [TestCase("\"a\" | is-single", false)]
    [TestCase("{{1}, {}, {1, 2}} | filter(is-single) | single | is-single", true)]
    public void Evaluate_Pipeline_ComposesThroughBinding(string source, bool expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(expected));

    [Test]
    public void Introspection_ExposesCanonicalNameWithoutAliases()
    {
        var info = new PredicateIntrospector().Describe().Single(x => x.ImplementationType == typeof(IsSingle));
        Assert.Multiple(() =>
        {
            Assert.That(info.Name, Is.EqualTo("is-single"));
            Assert.That(info.Scope, Is.EqualTo("array"));
            Assert.That(info.Aliases, Is.Empty);
            Assert.That(info.Parameters, Is.Empty);
        });
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, false)]
    public void Evaluate_Sequence_StopsAfterSecondElementAndDisposes(int count, bool expected)
    {
        var disposed = false;
        IEnumerable Values()
        {
            try
            {
                for (var index = 0; index < count; index++)
                    yield return index;
                if (count == 2)
                    throw new InvalidOperationException("Must not enumerate past the second element.");
            }
            finally
            {
                disposed = true;
            }
        }

        Assert.That(new IsSingle().Evaluate(Values()), Is.EqualTo(expected));
        Assert.That(disposed, Is.True);
    }
}
