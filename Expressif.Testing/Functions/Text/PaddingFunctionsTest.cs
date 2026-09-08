using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class PaddingFunctionsTest
{
    [TestCase(true, 2, "abc")]
    [TestCase(false, 2, "abc")]
    [TestCase(true, 5, "__abc")]
    [TestCase(false, 5, "abc__")]
    public void Padding_Arguments_EvaluatedOnlyWhenNeeded(bool left, int length, string expected)
    {
        var lengthEvaluations = 0;
        var characterEvaluations = 0;
        Func<int> getLength = () => { lengthEvaluations++; return length; };
        Func<char> getCharacter = () => { characterEvaluations++; return '_'; };
        BasePaddingFunction function = left ? new PadLeft(getLength, getCharacter) : new PadRight(getLength, getCharacter);

        var result = function.Evaluate("abc");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(lengthEvaluations, Is.EqualTo(1));
            Assert.That(characterEvaluations, Is.EqualTo(length > 3 ? 1 : 0));
        });
    }

    [Conformance]
    public void PadRight_Valid(object value, int length, char character, string expected)
        => Assert.That(new PadRight(() => (length), () => (character))
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PadLeft_Valid(string value, int length, char character, string expected)
        => Assert.That(new PadLeft(() => (length), () => (character))
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PadCenter_Valid(string value, int length, char character, string expected)
        => Assert.That(new PadCenter(() => (length), () => (character))
            .Evaluate(value), Is.EqualTo(expected));
}
