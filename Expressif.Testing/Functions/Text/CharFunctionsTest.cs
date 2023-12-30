using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text;

public class CharFunctionsTest
{
    [Test]
    [TestCase("123*456*78", "*", "12345678")]
    [TestCase("***123***456*78****", "*", "12345678")]
    [TestCase("******", "*", "")]
    [TestCase("(null)", "*", "(null)")]
    [TestCase(null, "*", "(null)")]
    [TestCase("(empty)", "*", "(empty)")]
    [TestCase("(blank)", "*", "(blank)")]
    [TestCase("(blank)", " ", "(empty)")]
    public void RemoveChars_Valid(string? value, char charToRemove, string expected)
        => Assert.That(new RemoveChars(() => (charToRemove)).Evaluate(value)
            , Is.EqualTo(expected));

    [Test]
    [TestCase("123*456*78", "*", "-", "123-456-78")]
    [TestCase("***123***456*78****", "*", "-", "---123---456-78----")]
    [TestCase("******", "*", "-", "------")]
    [TestCase("(null)", "*", "-", "(null)")]
    [TestCase(null, "*", "-", "(null)")]
    [TestCase("(empty)", "*", "-", "(empty)")]
    [TestCase("(blank)", "*", "-", "(blank)")]
    [TestCase("(blank)", " ", "-", "(blank)")]
    public void ReplaceChars_Valid(string? value, char charToReplace, char replacingChar, string expected)
        => Assert.That(new ReplaceChars(() => charToReplace, () => replacingChar).Evaluate(value)
            , Is.EqualTo(expected));


    [Test]
    [TestCase("12345678", new[] { '2', '8', '4' }, "248")]
    [TestCase("12314561789", new[] { '2', '1', '4' }, "12141")]
    [TestCase("(null)", new[] { '2', '1', '4' }, "(null)")]
    [TestCase(null, new[] { '2', '1', '4' }, "(null)")]
    [TestCase("(empty)", new[] { '2', '1', '4' }, "(empty)")]
    [TestCase("(blank)", new[] { '2', '1', '4' }, "(blank)")]
    [TestCase("(blank)", new[] { '2', '1', '4' }, "(blank)")]
    public void FilterChars_Chars_Valid(string? value, char[] filter, string expected)
        => Assert.That(new FilterChars(() => filter).Evaluate(value)
            , Is.EqualTo(expected));

    [Test]
    [TestCase("12345678", "284", "248")]
    [TestCase("12314561789", "214", "12141")]
    [TestCase("(null)", "214", "(null)")]
    [TestCase(null, "214", "(null)")]
    [TestCase("(empty)", "214", "(empty)")]
    [TestCase("(blank)", "214", "(blank)")]
    [TestCase("(blank)", "214", "(blank)")]
    public void FilterChars_String_Valid(string? value, string filter, string expected)
        => Assert.That(new FilterChars(() => filter).Evaluate(value)
            , Is.EqualTo(expected));

    [Test]
    [TestCase("foo", "foo")]
    [TestCase("foo bar", "foo bar")]
    [TestCase("foo  bar", "foo bar")]
    [TestCase("foo\tbar", "foo\tbar")]
    [TestCase("foo\t bar", "foo\tbar")]
    [TestCase("foo\r\nbar", "foo\r\nbar")]
    [TestCase("\t\t\tfoo\t\tbar\t\t\t", "foo\tbar")]
    [TestCase("(null)", "(null)")]
    [TestCase(null, "(null)")]
    [TestCase("(empty)", "(empty)")]
    [TestCase("(blank)", "(empty)")]
    public void CollapseWhitespace_Value_Valid(string? value, string expected)
        => Assert.That(new CollapseWhitespace().Evaluate(value)
            , Is.EqualTo(expected));

    [Test]
    [TestCase("foo", "foo")]
    [TestCase("foo bar", "foo bar")]
    [TestCase("foo  bar", "foo  bar")]
    [TestCase("foo\tbar", "foo bar")]
    [TestCase("foo\t bar", "foo  bar")]
    [TestCase("foo\r\nbar", "foo bar")]
    [TestCase("foo\rbar\n", "foo bar ")]
    [TestCase("foo\r\n\r\nbar", "foo  bar")]
    [TestCase("\t\t\tfoo\t\tbar\t\t\t", "   foo  bar   ")]
    [TestCase("(null)", "(null)")]
    [TestCase(null, "(null)")]
    [TestCase("(empty)", "(empty)")]
    [TestCase("(blank)", "(blank)")]
    public void CleanWhitespace_Value_Valid(string? value, string expected)
        => Assert.That(new CleanWhitespace().Evaluate(value)
            , Is.EqualTo(expected));
}
