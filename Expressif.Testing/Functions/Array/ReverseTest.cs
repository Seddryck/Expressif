using Expressif.Functions.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class ReverseTest
{
    [Conformance]
    public void Reverse_Valid(object input, object? expected)
        => Assert.That(new Reverse().Evaluate(input), Is.EqualTo(expected));
}

