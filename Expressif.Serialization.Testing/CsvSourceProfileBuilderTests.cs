namespace Expressif.Serialization.Testing;

public class CsvSourceProfileBuilderTests
{
    [Test]
    public void Build_AllSupportedOptions_TranslatesTypedValues()
    {
        var (profile, headersAreRows) = new CsvSourceProfileBuilder().Build(
        [
            new("delimiter", ";"), new("line-terminator", "|"), new("quote-char", null),
            new("double-quote", false), new("escape-char", "\\"), new("header", false),
            new("header-rows", new object?[] { 1, 3m }), new("header-join", "."), new("header-repeat", false),
            new("comment-char", "#"), new("comment-rows", new object?[] { 2, 4 }), new("null-sequence", "NULL"),
            new("missing-cell", "missing"), new("skip-initial-space", true),
            new("array-delimiter", ";"), new("array-prefix", "["), new("array-suffix", "]"),
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
            Assert.That(headersAreRows, Is.False);
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

    [Test]
    public void Build_RepeatedOptions_UsesLastValueWithoutRetainingState()
    {
        var builder = new CsvSourceProfileBuilder();
        Assert.That(builder.Build([new("delimiter", ";"), new("delimiter", "|")]).Profile.Dialect.Delimiter, Is.EqualTo('|'));
        Assert.That(builder.Build([]).Profile.Dialect.Delimiter, Is.EqualTo(','));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1.5)]
    [TestCase("1")]
    public void Build_InvalidRowIndexes_RejectsValue(object value)
        => Assert.That(() => new CsvSourceProfileBuilder().Build([new("header-rows", new[] { value })]), Throws.TypeOf<FormatException>().With.Message.Contains("one-based"));

    [Test]
    public void Build_EmptyRowIndexes_RejectsValue()
        => Assert.That(() => new CsvSourceProfileBuilder().Build([new("comment-rows", Array.Empty<object?>())]), Throws.TypeOf<FormatException>());
}
