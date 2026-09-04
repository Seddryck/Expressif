using System.Reflection;
using Expressif.Functions;
using Expressif.Functions.Text;
using Expressif.Testing.Conformance;
using Expressif.Values.Special;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class TextFunctionsTest
{
    [Conformance]
    public void Token_DefaultSeparator_Valid(string value, int index, string expected)
        => Assert.That(new Token(() => (index)).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Token_CustomSeparator_Valid(string value, int index, char separator, string expected)
    => Assert.That(new Token(() => (index), () => (separator))
        .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Tokenize_DefaultSeparator_Valid(object? value, string[] expected)
        => Assert.That(new Tokenize().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Tokenize_CustomSeparator_Valid(object? value, char separator, string[] expected)
        => Assert.That(new Tokenize(() => separator).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TokenizeLexical_Valid(object? value, string[] expected)
        => Assert.That(new TokenizeLexical().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TokenizeKebab_Valid(object? value, string[] expected)
        => Assert.That(new TokenizeKebab().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TokenizeSnake_Valid(object? value, string[] expected)
        => Assert.That(new TokenizeSnake().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TokenizeCamel_Valid(object? value, string[] expected)
        => Assert.That(new TokenizeCamel().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TokenizePascal_Valid(object? value, string[] expected)
        => Assert.That(new TokenizePascal().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TokenizeWords_Valid(object? value, string[] expected)
        => Assert.That(new TokenizeWords().Evaluate(value), Is.EqualTo(expected));

    [TestCase("\"first-name\" | tokenize-kebab | pascal-case", "FirstName")]
    [TestCase("\"first_name\" | tokenize-snake | kebab-case", "first-name")]
    [TestCase("\"firstName\" | tokenize-camel | snake-case", "first_name")]
    [TestCase("\"FirstName\" | tokenize-pascal | kebab-case", "first-name")]
    [TestCase("\"customer_HTTP-server_id\" | tokenize-words | pascal-case", "CustomerHttpServerId")]
    public void TokenizeNormalized_ComposesWithCasing(string expression, string expected)
        => Assert.That(new ExpressionFactory().Create(expression).Evaluate(null), Is.EqualTo(expected));

    [Test]
    public void Tokenize_ExposesClosedTypedContract()
    {
        IFunction<string?, string[]?> function = new Tokenize();

        Assert.That(function.Evaluate("foo bar"), Is.EqualTo(new[] { "foo", "bar" }));
    }

    [Test]
    [TestCase("abc 123")]
    [TestCase("abc 123 ")]
    [TestCase(" abc 123")]
    [TestCase("abc   123")]
    [TestCase("  abc   123  ")]
    [TestCase("  abc ,  123  ")]
    [TestCase("")]
    public void TokenCount_DefaultSeparator_Valid(string value)
    {
        var tokenCount = (int)new TokenCount().Evaluate(value)!;

        for (int i = 0; i < tokenCount; i++)
        {
            var nextToken = new Token(() => (i));
            Assert.That(nextToken.Evaluate(value), Is.Not.EqualTo(new Null()));
        }

        var token = new Token(() => (tokenCount));
        Assert.That(token.Evaluate(value), Is.EqualTo(new Null()));
    }

    [Test]
    [TestCase("abc-123")]
    [TestCase("abc 123 ")]
    [TestCase("-abc-123")]
    [TestCase("abc---123")]
    [TestCase("--abc---123--")]
    [TestCase("--abc-,--123--")]
    [TestCase("")]
    public void TokenCount_CustomSeparator_Valid(string value)
    {
        var tokenCount = (int)new TokenCount(() => ('-')).Evaluate(value)!;

        for (int i = 0; i < tokenCount; i++)
        {
            var nextToken = new Token(() => (i), () => ('-'));
            Assert.That(nextToken.Evaluate(value), Is.Not.EqualTo(new Null()));
        }

        var token = new Token(() => (tokenCount), () => ('-'));
        Assert.That(token.Evaluate(value), Is.EqualTo(new Null()));
    }

    [Test]
    [TestCase("")]
    [TestCase("\t")]
    [TestCase(" \t")]
    [TestCase(" ")]
    [TestCase("\r\n")]
    public void WhitespacesToEmpty_Empty(string value)
        => Assert.That(new WhitespacesToEmpty().Evaluate(value), Is.EqualTo(new Empty()));

    [Test]
    [TestCase(typeof(Empty))]
    [TestCase(typeof(Whitespace))]
    public void WhitespacesToEmpty_SpecialType_Empty(Type type)
    {
        var obj = type.GetConstructor([])!.Invoke(System.Array.Empty<Type>());
        Assert.That(new WhitespacesToEmpty().Evaluate(obj), Is.EqualTo(new Empty()));
    }

    [Test]
    [TestCase("foo")]
    [TestCase("(null)")]
    public void WhitespacesToEmpty_NotEmpty(string value)
        => Assert.That(new WhitespacesToEmpty().Evaluate(value), Is.Not.EqualTo(new Empty()));

    [Test]
    [TestCase(typeof(Null))]
    public void WhitespacesToEmpty_SpecialType_NotEmpty(Type type)
    {
        var obj = type.GetConstructor([])!.Invoke(System.Array.Empty<Type>());
        Assert.That(new WhitespacesToEmpty().Evaluate(obj), Is.Not.EqualTo(new Empty()));
    }

    [Test]
    [TestCase(typeof(DBNull))]
    public void NullToValue_DBNull_Null(Type type)
        => Assert.That(new WhitespacesToEmpty().Evaluate(
            type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
            , Is.Not.EqualTo(new Empty()));

    [Test]
    [TestCase("")]
    [TestCase("(null)")]
    [TestCase("\t")]
    [TestCase(" \t")]
    [TestCase(" ")]
    [TestCase("\r\n")]
    public void BlankToNull_Null(string value)
        => Assert.That(new WhitespacesToNull().Evaluate(value), Is.EqualTo(new Null()));

    [Test]
    [TestCase("foo")]
    public void BlankToNull_NotNull(string value)
        => Assert.That(new WhitespacesToNull().Evaluate(value), Is.Not.EqualTo(new Null()));

    [Test]
    [TestCase("")]
    [TestCase("(null)")]
    [TestCase("(empty)")]
    public void EmptyToNull_Null(string value)
        => Assert.That(new EmptyToNull().Evaluate(value), Is.EqualTo(new Null()));

    [Test]
    [TestCase("alpha")]
    [TestCase("\t")]
    [TestCase(" \t")]
    [TestCase(" ")]
    [TestCase("\r\n")]
    public void EmptyToNull_NotNull(string value)
        => Assert.That(new EmptyToNull().Evaluate(value), Is.Not.EqualTo(new Null()));

    [Test]
    [TestCase("")]
    [TestCase("(null)")]
    [TestCase("(empty)")]
    public void NullToEmpty_Null(string value)
        => Assert.That(new NullToEmpty().Evaluate(value), Is.EqualTo(new Empty()));

    [Test]
    [TestCase("foo")]
    [TestCase("(blank)")]
    public void NullToEmpty_NotNull(string value)
        => Assert.That(new NullToEmpty().Evaluate(value), Is.Not.EqualTo(new Null()));

    [Conformance]
    public void Trim_Valid(object value, object expected)
        => Assert.That(new Trim().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TextToHtml_Valid(object value, object expected)
        => Assert.That(new TextToHtml().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HtmlToText_Valid(object value, object expected)
        => Assert.That(new HtmlToText().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TextToUri_Valid(object? value, object? expected)
        => Assert.That(new TextToUri().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void UriToText_Valid(object? value, object? expected)
        => Assert.That(new UriToText().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TextToJsonEscaped_Valid(object? value, object? expected)
        => Assert.That(new TextToJsonEscaped().Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void TextToJsonEscaped_InvalidUtf16_ReturnsNull()
        => Assert.That(new TextToJsonEscaped().Evaluate("\uD800"), Is.Null);

    [Conformance]
    public void JsonEscapedToText_Valid(object? value, object? expected)
        => Assert.That(new JsonEscapedToText().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void JsonEscapedToText_Invalid(object? value, object? expected)
        => Assert.That(new JsonEscapedToText().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TextToXmlEscaped_Valid(object? value, object? expected)
        => Assert.That(new TextToXmlEscaped().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TextToXmlEscaped_Invalid(object? value, object? expected)
        => Assert.That(new TextToXmlEscaped().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void XmlEscapedToText_Valid(object? value, object? expected)
        => Assert.That(new XmlEscapedToText().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void XmlEscapedToText_Invalid(object? value, object? expected)
        => Assert.That(new XmlEscapedToText().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void WithoutDiacritics_Valid(object value, object expected)
        => Assert.That(new WithoutDiacritics().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void WithoutWhitespaces_Valid(object? value, object? expected)
        => Assert.That(new WithoutWhitespaces().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FirstChars_Valid(string value, int length, string expected)
        => Assert.That(new FirstChars(() => (length)).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void LastChars_Valid(string value, int length, string expected)
        => Assert.That(new LastChars(() => (length)).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void SkipFirstChars_Valid(string value, int length, string expected)
        => Assert.That(new SkipFirstChars(() => (length)).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void SkipLastChars_Valid(string value, int length, string expected)
        => Assert.That(new SkipLastChars(() => (length)).Evaluate(value)
            , Is.EqualTo(expected));

    [Test]
    [TestCase("20190317111223", "yyyyMMddhhmmss", "2019-03-17 11:12:23")]
    [TestCase("2019-03-17 11:12:23", "yyyy-MM-dd hh:mm:ss", "2019-03-17 11:12:23")]
    [TestCase("17-03-2019 11:12:23", "dd-MM-yyyy hh:mm:ss", "2019-03-17 11:12:23")]
    [TestCase("2019-03-17T11:12:23", "yyyy-MM-ddThh:mm:ss", "2019-03-17 11:12:23")]
    [TestCase("17/03/2019 11:12:23", "dd/MM/yyyy hh:mm:ss", "2019-03-17 11:12:23")]
    [TestCase("17.03.2019 11.12.23", "dd.MM.yyyy hh.mm.ss", "2019-03-17 11:12:23")]
    [TestCase("Wed, 25.09.19", "ddd, dd.MM.yy", "2019-09-25")]
    [TestCase("Wednesday 25-SEP-19", "dddd dd-MMM-yy", "2019-09-25")]
    [TestCase("2019-10-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "2019-10-01 19:58:00")]
    public void TextToDateTime_Valid(string value, string format, DateTime expected)
    {
        var function = new TextToDateTime(() => (format));
        var result = function.Evaluate(value);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(((DateTime)result!).Kind, Is.EqualTo(DateTimeKind.Unspecified));
        });
    }

    [Conformance]
    public void TextToDateTime_Valid_Culture(string value, string format, string culture, DateTime expected)
        => Assert.That(new TextToDateTime(() => (format), () => (culture))
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TextToMask_Valid(string value, string mask, string expected)
        => Assert.That(new TextToMask(() => (mask)).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void MaskToText_Valid(string value, string mask, string expected)
        => Assert.That(new MaskToText(() => (mask)).Evaluate(value)
            , Is.EqualTo(expected));
}
