using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Testing.Values.Casters;

public class ArrayCasterTest
{
    [Test]
    public void TryCast_StringArrayLiteral_Success()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new ArrayCaster().TryCast("{1,2,2}", out var value), Is.True);
            Assert.That(value, Is.EqualTo(new object?[] { 1, 2, 2 }));
        });
    }

    [Test]
    public void TryCast_StringNestedArrayLiteral_Success()
    {
        Assert.That(new ArrayCaster().TryCast("{{1,2},{name := `alice`}}", out var value), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(value, Has.Length.EqualTo(2));
            Assert.That(value![0], Is.EqualTo(new object?[] { 1, 2 }));
            Assert.That(value[1], Is.TypeOf<RecordValue>());
            Assert.That(((RecordValue)value[1]!)["name"], Is.EqualTo("alice"));
        });
    }

    [Test]
    public void TryCast_Enumerable_Success()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new ArrayCaster().TryCast(new List<object?> { 1, "2", null }, out var value), Is.True);
            Assert.That(value, Is.EqualTo(new object?[] { 1, "2", null }));
        });
    }

    [Test]
    public void TryCast_ScalarString_Failure()
        => Assert.That(new ArrayCaster().TryCast("abc", out _), Is.False);
}
