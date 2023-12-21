using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text;

public class CasingTest
{
    [TestCase("Foobar", false)]
    [TestCase("foobar", true)]
    [TestCase("FOOBAR", false)]
    [TestCase("Foo Bar", false)]
    [TestCase("foo bar", true)]
    [TestCase("FOO BAR", false)]
    [TestCase("", true)]
    [TestCase("(empty)", true)]
    [TestCase("(null)", true)]
    [TestCase(null, true)]
    public void LowerCase_Text_Success(object? value, bool expected)
        => Assert.That(new LowerCase().Evaluate(value), Is.EqualTo(expected));

    [TestCase("Foobar", false)]
    [TestCase("foobar", false)]
    [TestCase("FOOBAR", true)]
    [TestCase("Foo Bar", false)]
    [TestCase("foo bar", false)]
    [TestCase("FOO BAR", true)]
    [TestCase("", true)]
    [TestCase("(empty)", true)]
    [TestCase("(null)", true)]
    [TestCase(null, true)]
    public void UpperCase_Text_Success(object? value, bool expected)
        => Assert.That(new UpperCase().Evaluate(value), Is.EqualTo(expected));
}
