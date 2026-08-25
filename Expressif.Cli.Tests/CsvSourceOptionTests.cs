using Expressif.Cli.Application;
using Expressif.Cli.Inputs;

namespace Expressif.Cli.Tests;

public class CsvSourceOptionTests
{
    private readonly CliInputValueParser parser = new();
    private readonly RunHandler handler = new(CliServices.CreateDefault());

    [Test]
    public void Parse_PlainScalar_FallsBackToTrimmedText()
        => Assert.That(parser.Parse("  nikola  "), Is.EqualTo("nikola"));

    [Test]
    public void Parse_UnprefixedPrimitiveName_FallsBackToText()
    {
        Assert.Multiple(() =>
        {
            Assert.That(parser.Parse("null"), Is.EqualTo("null"));
            Assert.That(parser.Parse("true"), Is.EqualTo("true"));
            Assert.That(parser.Parse("false"), Is.EqualTo("false"));
        });
    }

    [Test]
    public void Parse_IsoDate_PreservesDateType()
        => Assert.That(
            parser.Parse("  2026-08-23  "),
            Is.EqualTo(new DateOnly(2026, 8, 23)).And.TypeOf<DateOnly>());

    [Test]
    public void Parse_ExplicitPrimitives_PreserveTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(parser.Parse("#null"), Is.Null);
            Assert.That(parser.Parse("#true"), Is.True);
            Assert.That(parser.Parse("#false"), Is.False);
        });
    }

    [TestCase("  {1, 2")]
    [TestCase("  T(1, 2")]
    public void Parse_MalformedStructuredInput_DoesNotFallBackToText(string value)
        => Assert.Throws<FormatException>(() => parser.Parse(value));

    [Test]
    public void BuildCsvProfile_AllSupportedOptions_AreTranslated()
    {
        var (profile, hasHeader) = handler.BuildCsvProfile(
        [
            "delimiter=\";\"", "line-terminator=\"|\"", "quote-char=#null",
            "double-quote=#false", "escape-char=\"\\\\\"", "header=#false",
            "header-rows={1, 3}", "header-join=\".\"", "header-repeat=#false",
            "comment-char=\"#\"", "comment-rows={2, 4}", "null-sequence=\"NULL\"",
            "missing-cell=\"missing\"", "skip-initial-space=#true",
            "array-delimiter=\";\"", "array-prefix=\"[\"", "array-suffix=\"]\""
        ]);

        var dialect = profile.Dialect;
        Assert.Multiple(() =>
        {
            Assert.That(dialect.Delimiter, Is.EqualTo(';'));
            Assert.That(dialect.LineTerminator, Is.EqualTo("|"));
            Assert.That(dialect.QuoteChar, Is.Null);
            Assert.That(dialect.DoubleQuote, Is.False);
            Assert.That(dialect.EscapeChar, Is.EqualTo('\\'));
            Assert.That(dialect.Header, Is.False);
            Assert.That(hasHeader, Is.False);
            Assert.That(dialect.HeaderRows, Is.EqualTo(new[] { 1, 3 }));
            Assert.That(dialect.HeaderJoin, Is.EqualTo("."));
            Assert.That(dialect.HeaderRepeat, Is.False);
            Assert.That(dialect.CommentChar, Is.EqualTo('#'));
            Assert.That(dialect.CommentRows, Is.EqualTo(new[] { 2, 4 }));
            Assert.That(dialect.NullSequence, Is.EqualTo("NULL"));
            Assert.That(dialect.MissingCell, Is.EqualTo("missing"));
            Assert.That(dialect.SkipInitialSpace, Is.True);
            Assert.That(dialect.ArrayDelimiter, Is.EqualTo(';'));
            Assert.That(dialect.ArrayPrefix, Is.EqualTo('['));
            Assert.That(dialect.ArraySuffix, Is.EqualTo(']'));
        });
    }

    [TestCase("unknown=1", "Unknown CSV source option 'unknown' with value '1'.")]
    [TestCase("delimiter=\"long\"", "Invalid CSV source option 'delimiter' with value '\"long\"'")]
    [TestCase("header=\"true\"", "Invalid CSV source option 'header' with value '\"true\"'")]
    [TestCase("header-rows={0}", "Invalid CSV source option 'header-rows' with value '{0}'")]
    public void BuildCsvProfile_InvalidOption_IdentifiesNameAndValue(string option, string expected)
    {
        var exception = Assert.Throws<FormatException>(() => handler.BuildCsvProfile([option]));
        Assert.That(exception!.Message, Does.StartWith(expected));
    }
}
