using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Sorting;

public class SortByTest
{
    [Conformance]
    public void SortBy_Valid_Criteria(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void SortBy_EqualKeys_IsStable()
    {
        var source = "array({id := 1, key := 0}, {id := 2, key := 0}) | sort-by(.key -> :numeric)";
        var result = (object?[])Expression.Create(source).Evaluate(null)!;
        Assert.That(result.Select(item => NamedValueAccessor.Get(item, "id")), Is.EqualTo(new object[] { 1m, 2m }));
    }

    [Test]
    public void SortBy_DirectTupleProjection_EvaluatesAgainstEachItem()
    {
        var source = "array(T(1, 100), T(2, 120), T(4, 75), T(3, 110)) | sort-by($0 -> :integer)";
        Assert.That(
            ValueFormatter.Format(Expression.Create(source).Evaluate(null)),
            Is.EqualTo("{T(1, 100), T(2, 120), T(3, 110), T(4, 75)}"));
    }

    [Test]
    public void SortBy_MissingCriteria_Throws()
        => Assert.That(() => Expression.Create("array() | sort-by()").Evaluate(null), Throws.TypeOf<BindingException>());

    [Test]
    public void SortBy_InvalidCoercion_BecomesNullKey()
        => Assert.That(
            ValueFormatter.Format(Expression.Create("array({v := \"bad\"}, {v := 1}) | sort-by(.v -> :numeric)").Evaluate(null)),
            Is.EqualTo("{{v := 1}, {v := \"bad\"}}"));

    [Test]
    public void SortBy_UnsupportedType_ThrowsClearly()
        => Assert.That(
            () => Expression.Create("array(1) | sort-by(neutral -> :boolean)").Evaluate(null),
            Throws.TypeOf<BindingException>().With.Message.Contains(":boolean"));
}
