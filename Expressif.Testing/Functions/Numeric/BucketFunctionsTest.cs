using System;
using Expressif;
using Expressif.Functions;
using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class BucketFunctionsTest
{
    [Conformance]
    public void Bucket_Valid(object? value, decimal minimum, decimal maximum, int count, int? expected)
        => Assert.That(
            new Bucket(() => minimum, () => maximum, () => count).Evaluate(value),
            Is.EqualTo(expected));

    [Conformance]
    public void BucketWithOutliers_Valid(object? value, decimal minimum, decimal maximum, int count, int? expected)
        => Assert.That(
            new BucketWithOutliers(() => minimum, () => maximum, () => count).Evaluate(value),
            Is.EqualTo(expected));

    [Test]
    public void Bucket_TypedAndUntypedEntryPoints_AreEquivalent()
    {
        IFunction<decimal?, int?> typed = new Bucket(() => 0, () => 20, () => 4);
        IFunction untyped = typed;

        Assert.Multiple(() =>
        {
            Assert.That(typed.Evaluate(7.5m), Is.EqualTo(2));
            Assert.That(untyped.Evaluate(7.5m), Is.EqualTo(typed.Evaluate(7.5m)));
            Assert.That(typed.Evaluate(null), Is.EqualTo(untyped.Evaluate(null)));
        });
    }

    [Test]
    public void Bucket_UntypedEntryPoint_PreservesNumericCoercion()
    {
        IFunction function = new Bucket(() => 0, () => 20, () => 4);

        Assert.Multiple(() =>
        {
            Assert.That(function.Evaluate("7.5"), Is.EqualTo(2));
            Assert.That(function.Evaluate(DBNull.Value), Is.Null);
        });
    }

    [TestCase("bucket(5000, 20000, 3)", 12500, 2)]
    [TestCase("bucket-with-outliers(5000, 20000, 3)", 22500, 4)]
    public void BucketFunctions_BindAndEvaluateByCanonicalName(string code, int value, int expected)
        => Assert.That(Expression.Create(code).Evaluate(value), Is.EqualTo(expected));
}
