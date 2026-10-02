using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Set;

[TestFixture]
public class DistinctTest
{
    [Conformance]
    public void Distinct_Valid(object? value, object? expected)
        => Assert.That(new Distinct().Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class DifferenceTest
{
    [Conformance]
    public void Difference_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new Difference(() => array).Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class SymmetricDifferenceTest
{
    [Conformance]
    public void SymmetricDifference_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new SymmetricDifference(() => array).Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class ComplementTest
{
    [Conformance]
    public void Complement_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new Complement(() => array).Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class IntersectionTest
{
    [Conformance]
    public void Intersection_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new Intersection(() => array).Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class UnionTest
{
    [Conformance]
    public void Union_Valid_Array(object? value, object?[] array, object? expected)
        => Assert.That(new Union(() => array).Evaluate(value), Is.EqualTo(expected));
}
