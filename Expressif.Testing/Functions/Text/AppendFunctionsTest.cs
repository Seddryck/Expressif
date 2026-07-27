using Expressif.Functions.Text;
using Expressif.Testing.Conformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class AppendFunctionsTest
{
    [Conformance]
    public void Prefix_Valid(string value, string prefix, string expected)
        => Assert.That(new Prefix(() => prefix).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void Suffix_Valid(string value, string suffix, string expected)
        => Assert.That(new Suffix(() => suffix).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void Prepend_Valid(string value, string prepend, string expected)
        => Assert.That(new Prepend(() => prepend).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void Append_Valid(string value, string append, string expected)
        => Assert.That(new Append(() => append).Evaluate(value)
            , Is.EqualTo(expected));

    #region Space

    [Conformance]
    public void PrefixSpace_Valid(string value, string expected)
        => Assert.That(new PrefixSpace().Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void SuffixSpace_Valid(string value, string expected)
        => Assert.That(new SuffixSpace().Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void PrependSpace_Valid(string value, string expected)
        => Assert.That(new PrependSpace().Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void AppendSpace_Valid(string value, string expected)
        => Assert.That(new AppendSpace().Evaluate(value)
            , Is.EqualTo(expected));

    #endregion

    #region NewLine

    [Test]
    [TestCase("123456789", "#NewLine#123456789")]
    [TestCase("(null)", "(null)")]
    public void PrefixNewLine_Valid(string value, string expected)
        => Assert.That(new PrefixNewLine().Evaluate(value)
            , Is.EqualTo(expected.Replace("#NewLine#", Environment.NewLine)));

    [Test]
    [TestCase("123456789", "123456789#NewLine#")]
    [TestCase("(null)", "(null)")]
    public void SuffixNewLine_Valid(string value, string expected)
        => Assert.That(new SuffixNewLine().Evaluate(value)
            , Is.EqualTo(expected.Replace("#NewLine#", Environment.NewLine)));

    [Test]
    [TestCase("123456789", "#NewLine#123456789")]
    [TestCase("(null)", "#NewLine#")]
    public void PrependNewLine_Valid(string value, string expected)
        => Assert.That(new PrependNewLine().Evaluate(value)
            , Is.EqualTo(expected.Replace("#NewLine#", Environment.NewLine)));

    [Test]
    [TestCase("123456789", "123456789#NewLine#")]
    [TestCase("(null)", "#NewLine#")]
    public void AppendNewLine_Valid(string value, string expected)
        => Assert.That(new AppendNewLine().Evaluate(value)
            , Is.EqualTo(expected.Replace("#NewLine#", Environment.NewLine)));

    #endregion

    [Conformance]
    public void ReplaceSlice_Valid(string value, int start, int length, string append, string expected)
        => Assert.That(new ReplaceSlice(() => start, () => length, () => append).Evaluate(value)
            , Is.EqualTo(expected));
}
