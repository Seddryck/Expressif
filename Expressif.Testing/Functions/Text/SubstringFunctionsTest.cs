using Expressif.Functions.Text;
using NUnit.Framework.Internal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class SubstringFunctionsTest
{
    [Conformance]
    public void BeforeSubstring_Valid(string value, string substring, string expected)
        => Assert.That(new BeforeSubstring(() => substring).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void BeforeSubstring_Position_Valid(string value, string substring, int position, string expected)
        => Assert.That(new BeforeSubstring(() => substring, () => position).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void AfterSubstring_Valid(string value, string substring, string expected)
        => Assert.That(new AfterSubstring(() => substring).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void AfterSubstring_Position_Valid(string value, string substring, int position, string expected)
        => Assert.That(new AfterSubstring(() => substring, () => position).Evaluate(value)
            , Is.EqualTo(expected));
}
