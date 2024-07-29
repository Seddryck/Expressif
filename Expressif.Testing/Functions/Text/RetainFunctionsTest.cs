using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text;

public class RetainFunctionsTest
{
    [Test]
    [TestCase("abc12345abc6789", "123456789")]
    [TestCase("abc12345abc, 6789", "123456789")]
    [TestCase("12345", "12345")]
    [TestCase("-12,345.6", "123456")]
    [TestCase("abc", "(empty)")]
    [TestCase("(null)", "(null)")]
    [TestCase("(empty)", "(empty)")]
    [TestCase("(blank)", "(empty)")]
    public void RetainNumeric_Valid(string value, string expected)
       => Assert.That(new RetainNumeric().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase("abc12345abc6789", "123456789")]
    [TestCase("abc12345abc, 6789", "12345,6789")]
    [TestCase("12345", "12345")]
    [TestCase("-12,345.6", "-12,345.6")]
    [TestCase("abc", "(empty)")]
    [TestCase("(null)", "(null)")]
    [TestCase("(empty)", "(empty)")]
    [TestCase("(blank)", "(empty)")]
    public void RetainNumericSymbol_Valid(string value, string expected)
        => Assert.That(new RetainNumericSymbol().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase("abc12345abc6789", "abcabc")]
    [TestCase("abc12345abc, 6789", "abcabc")]
    [TestCase("12345", "(empty)")]
    [TestCase("-12,345.6", "(empty)")]
    [TestCase("abc", "abc")]
    [TestCase("(null)", "(null)")]
    [TestCase("(empty)", "(empty)")]
    [TestCase("(blank)", "(empty)")]
    public void RetainAlpha_Valid(string value, string expected)
        => Assert.That(new RetainAlpha().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase("abc12345abc6789", "abc12345abc6789")]
    [TestCase("abc12345abc, 6789", "abc12345abc6789")]
    [TestCase("12345", "12345")]
    [TestCase("-12,345.6", "123456")]
    [TestCase("abc", "abc")]
    [TestCase("(null)", "(null)")]
    [TestCase("(empty)", "(empty)")]
    [TestCase("(blank)", "(empty)")]
    public void RetainAlphaNumeric_Valid(string value, string expected)
        => Assert.That(new RetainAlphaNumeric().Evaluate(value), Is.EqualTo(expected));
}
