using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Text;

public class CountingFunctionsTest
{
    [Test]
    [TestCase("foo", 3)]
    [TestCase(" foo ", 5)]
    [TestCase("", 0)]
    [TestCase("(null)", 0)]
    [TestCase("(empty)", 0)]
    [TestCase("(blank)", -1)]
    public void Length_Valid(object value, int expected)
        => Assert.That(new Length().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase("foo", 2)]
    [TestCase("foobarfoo", 5)]
    [TestCase("barfoobar", 5)]
    [TestCase("FOOfoo", 4)]
    [TestCase("(null)", 0)]
    [TestCase("(empty)", 0)]
    [TestCase("(blank)", -1)]
    public void CountDistinctChars_Valid(object value, int expected)
        => Assert.That(new CountDistinctChars().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase("foo", "foo", 1 )]
    [TestCase("foobar", "foo", 1)]
    [TestCase("barfoobar", "foo", 1)]
    [TestCase("barfoo", "foo", 1)]
    [TestCase("barfoobarfoobar", "foo", 2)]
    [TestCase("barfoobarfoobar", "bar", 3)]
    [TestCase("---*#*#*---", "*#*", 1)]
    [TestCase("(null)", "foo", 0)]
    [TestCase("(empty)", "foo", 0)]
    [TestCase("(blank)", "foo", -1)]
    public void CountSubstring_Valid(object value, string substring, int expected)
        => Assert.That(new CountSubstring(() => substring).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase("My taylor is rich", 4)]
    [TestCase(" My Lord ! ", 2)]
    [TestCase("  My     Lord    !   ", 2)]
    [TestCase("  My     Lord    !   C-L.", 3)]
    [TestCase("(null)", 0)]
    [TestCase(null, 0)]
    [TestCase("(empty)", 0)]
    [TestCase("(blank)", 0)]
    [TestCase("1 2017-07-06      CUST0001", 3)]
    [TestCase("1 2017-07-06          CUST0001", 3)]
    public void TokenCount_Valid(object? value, int expected)
        => Assert.That(new TokenCount().Evaluate(value), Is.EqualTo(expected));
}
