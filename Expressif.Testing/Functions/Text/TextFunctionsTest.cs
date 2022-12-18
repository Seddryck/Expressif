using Expressif.Functions.Text;
using Expressif.Values;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Expressif.Testing.Functions.Text
{
    [TestFixture]
    public class TextFunctionsTest
    {
        [Test]
        [TestCase(0, "1 2017-07-06      CUST0001", "1")]
        [TestCase(1, "1 2017-07-06      CUST0001", "2017-07-06")]
        [TestCase(2, "1 2017-07-06      CUST0001", "CUST0001")]
        [TestCase(2, "1 2017-07-06  ,    CUST0001", "CUST0001")]
        [TestCase(2, "1 2017-07-06          CUST0001", "CUST0001")]
        [TestCase(100, "1 2017-07-06      CUST0001", "(null)")]
        [TestCase(0, "(null)", "(null)")]
        [TestCase(0, "(blank)", "(null)")]
        [TestCase(0, "(empty)", "(null)")]
        public void Token_DefaultSeparator_Valid(int index, string value, string expected)
            => Assert.That(new Token(new LiteralScalarResolver<int>(index)).Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(0, ';', "1;2017-07-06;CUST0001", "1")]
        [TestCase(1, ',', "1,      2017-07-06 ,CUST0001", "      2017-07-06 ")]
        [TestCase(2, '|', "1 | 2017-07-06 | CUST0001", " CUST0001")]
        [TestCase(0, '|', "(null)", "(null)")]
        [TestCase(0, '|', "(blank)", "(blank)")]
        [TestCase(0, ' ', "(blank)", "(null)")]
        [TestCase(0, '|', "(empty)", "(null)")]
        public void Token_CustomSeparator_Valid(int index, char separator, string value, string expected)
        => Assert.That(new Token(new LiteralScalarResolver<int>(index), new LiteralScalarResolver<char>(separator))
            .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("abc 123")]
        [TestCase("abc 123 ")]
        [TestCase(" abc 123")]
        [TestCase("abc   123")]
        [TestCase("  abc   123  ")]
        [TestCase("  abc ,  123  ")]
        public void TokenCount_DefaultSeparator_Valid(string value)
        {
            var tokenCount = (int)new TokenCount().Evaluate(value)!;

            for (int i = 0; i < tokenCount; i++)
            {
                var nextToken = new Token(new LiteralScalarResolver<int>(i));
                Assert.That(nextToken.Evaluate(value), Is.Not.EqualTo(new Null()));
            }

            var Token = new Token(new LiteralScalarResolver<int>(tokenCount));
            Assert.That(Token.Evaluate(value), Is.EqualTo(new Null()));
        }

        [Test]
        [TestCase("")]
        [TestCase("\t")]
        [TestCase(" \t")]
        [TestCase(" ")]
        [TestCase("\r\n")]
        public void BlankToEmpty_Empty(string value)
            => Assert.That(new WhitespacesToEmpty().Evaluate(value), Is.EqualTo(new Empty()));

        [Test]
        [TestCase("foo")]
        [TestCase("(null)")]
        public void BlankToEmpty_NotEmpty(string value)
            => Assert.That(new WhitespacesToEmpty().Evaluate(value), Is.Not.EqualTo(new Empty()));

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

        [Test]
        [TestCase("FOO", "FOO")]
        [TestCase("foo", "foo")]
        [TestCase(" foO", "foO")]
        [TestCase("    foO", "foO")]
        [TestCase("foO ", "foO")]
        [TestCase("foO    ", "foO")]
        [TestCase("(null)", "(null)")]
        [TestCase("(empty)", "(empty)")]
        [TestCase("(blank)", "(empty)")]
        public void Trim_Valid(object value, object expected)
            => Assert.That(new Trim().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("FOO", "FOO")]
        [TestCase("foo", "FOO")]
        [TestCase("fOo", "FOO")]
        [TestCase(" foO ", " FOO ")]
        [TestCase("(null)", "(null)")]
        [TestCase("(empty)", "(empty)")]
        [TestCase("(blank)", "(blank)")]
        public void Upper_Valid(object value, object expected)
            => Assert.That(new Upper().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("FOO", "foo")]
        [TestCase("foo", "foo")]
        [TestCase("fOo", "foo")]
        [TestCase(" foO ", " foo ")]
        [TestCase("(null)", "(null)")]
        [TestCase("(empty)", "(empty)")]
        [TestCase("(blank)", "(blank)")]
        public void Lower_Valid(object value, object expected)
            => Assert.That(new Lower().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("foo", 3)]
        [TestCase(" foo ", 5)]
        [TestCase("", 0)]
        [TestCase("(null)", 0)]
        [TestCase("(empty)", 0)]
        [TestCase("(blank)", -1)]
        public void Length_Valid(object value, int length)
            => Assert.That(new Length().Evaluate(value), Is.EqualTo(length));

        [Test]
        [TestCase("Cédric")]
        public void TextToHtml_Valid(object value)
            => Assert.That(new TextToHtml().Evaluate(value), Is.EqualTo("C&#233;dric"));

        [Test]
        [TestCase("C&#233;dric")]
        [TestCase("C&eacute;dric")]
        public void HtmlToText_Valid(object value)
            => Assert.That(new HtmlToText().Evaluate(value), Is.EqualTo("Cédric"));

        [Test]
        [TestCase("Cédric")]
        [TestCase("Cèdric")]
        [TestCase("Cêdric")]
        [TestCase("Cedrîc")]
        [TestCase("Cedrïc")]
        [TestCase("Cedriç")]
        [TestCase("Cedrìc")]
        public void WithoutDiacritics_Valid(object value)
            => Assert.That(new WithoutDiacritics().Evaluate(value), Is.EqualTo("Cedric"));

        [Test]
        [TestCase("My taylor is rich", "Mytaylorisrich")]
        [TestCase(" My Lord ! ", "MyLord!")]
        [TestCase("My Lord !\r\nMy taylor is \t rich", "MyLord!Mytaylorisrich")]
        [TestCase("(null)", "(null)")]
        [TestCase(null, "(null)")]
        [TestCase("(empty)", "(empty)")]
        [TestCase("(blank)", "(empty)")]
        public void WithoutWhitespaces_Valid(object value, string expected)
            => Assert.That(new WithoutWhitespaces().Evaluate(value), Is.EqualTo(expected));

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
        public void TokenCount_Valid(object value, int expected)
            => Assert.That(new TokenCount().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", "abc", "abc123456789")]
        [TestCase("(null)", "abc", "(null)")]
        [TestCase("(empty)", "abc", "abc")]
        [TestCase("(blank)", "abc", "abc")]
        public void Prefix_Valid(string value, string prefix, string expected)
            => Assert.That(new Prefix(new LiteralScalarResolver<string>(prefix)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", "abc", "123456789abc")]
        [TestCase("(null)", "abc", "(null)")]
        [TestCase("(empty)", "abc", "abc")]
        [TestCase("(blank)", "abc", "abc")]
        public void Suffix_Valid(string value, string suffix, string expected)
            => Assert.That(new Suffix(new LiteralScalarResolver<string>(suffix)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", 9, "123456789")]
        [TestCase("123456789", 10, "123456789")]
        [TestCase("123456789", 8, "12345678")]
        [TestCase("123456789", 0, "")]
        [TestCase("(null)", 3, "(null)")]
        [TestCase("(empty)", 3, "(empty)")]
        public void FirstChars_Valid(string value, int length, string expected)
            => Assert.That(new FirstChars(new LiteralScalarResolver<int>(length)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", 9, "123456789")]
        [TestCase("123456789", 10, "123456789")]
        [TestCase("123456789", 8, "23456789")]
        [TestCase("123456789", 0, "")]
        [TestCase("(null)", 3, "(null)")]
        [TestCase("(empty)", 3, "(empty)")]
        public void LastChars_Valid(string value, int length, string expected)
            => Assert.That(new LastChars(new LiteralScalarResolver<int>(length)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", 9, "(empty)")]
        [TestCase("123456789", 10, "(empty)")]
        [TestCase("123456789", 8, "9")]
        [TestCase("123456789", 5, "6789")]
        [TestCase("123456789", 0, "123456789")]
        [TestCase("(null)", 3, "(null)")]
        [TestCase("(empty)", 3, "(empty)")]
        public void SkipFirstChars_Valid(string value, int length, string expected)
            => Assert.That(new SkipFirstChars(new LiteralScalarResolver<int>(length)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("123456789", 9, "(empty)")]
        [TestCase("123456789", 10, "(empty)")]
        [TestCase("123456789", 8, "1")]
        [TestCase("123456789", 5, "1234")]
        [TestCase("123456789", 0, "123456789")]
        [TestCase("(null)", 3, "(null)")]
        [TestCase("(empty)", 3, "(empty)")]
        public void SkipLastChars_Valid(string value, int length, string expected)
            => Assert.That(new SkipLastChars(new LiteralScalarResolver<int>(length)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("1234", 9, '0', "123400000")]
        [TestCase("1234", 9, '*', "1234*****")]
        [TestCase("123456789", 3, '0', "123456789")]
        [TestCase("(null)", 3, '0', "000")]
        [TestCase("(empty)", 3, '0', "000")]
        public void PadRight_Valid(string value, int length, char character, string expected)
            => Assert.That(new PadRight(new LiteralScalarResolver<int>(length), new LiteralScalarResolver<char>(character))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("1234", 9, '0', "000001234")]
        [TestCase("1234", 9, '*', "*****1234")]
        [TestCase("123456789", 3, '0', "123456789")]
        [TestCase("(null)", 3, '0', "000")]
        [TestCase("(empty)", 3, '0', "000")]
        public void PadLeft_Valid(string value, int length, char character, string expected)
            => Assert.That(new PadLeft(new LiteralScalarResolver<int>(length), new LiteralScalarResolver<char>(character))
                .Evaluate(value), Is.EqualTo(expected));

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
            var function = new TextToDateTime(new LiteralScalarResolver<string>(format));
            var result = function.Evaluate(value);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expected));
                Assert.That(((DateTime)result!).Kind, Is.EqualTo(DateTimeKind.Unspecified));
            });
        }

        [Test]
        [TestCase("20190317111223", "yyyyMMddhhmmss", "fr-fr", "2019-03-17 11:12:23")]
        [TestCase("mercredi 25-sept.-19", "dddd dd-MMM-yy", "fr-fr", "2019-09-25")]
        public void TextToDateTime_Culture_Valid(string value, string format, string culture, DateTime expected)
            => Assert.That(new TextToDateTime(new LiteralScalarResolver<string>(format), new LiteralScalarResolver<string>(culture))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("123*456*78", "*", "12345678")]
        [TestCase("***123***456*78****", "*", "12345678")]
        [TestCase("******", "*", "")]
        [TestCase("(null)", "*", "(null)")]
        [TestCase("(empty)", "*", "(empty)")]
        [TestCase("(blank)", "*", "(blank)")]
        [TestCase("(blank)", " ", "(empty)")]
        public void RemoveChars_Valid(string value, char charToRemove, string expected)
            => Assert.That(new RemoveChars(new LiteralScalarResolver<char>(charToRemove)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("12345678", "BE-***.***.**", "BE-123.456.78")]
        [TestCase("1234567890", "BE-***.***.**", "BE-123.456.78")]
        [TestCase("12345", "BE-***.***.**", "BE-123.45*.**")]
        [TestCase("(null)", "BE-***.***.**", "(null)")]
        [TestCase("(empty)", "BE-***.***.**", "BE-***.***.**")]
        [TestCase("(blank)", "BE-***.***.**", "BE-***.***.**")]
        public void TextToMask_Valid(string value, string mask, string expected)
            => Assert.That(new TextToMask(new LiteralScalarResolver<string>(mask)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("12345678", "BE-***.***.**", "BE-123.456.78")]
        [TestCase("12345", "BE-***.***.**", "BE-123.45*.**")]
        [TestCase("(null)", "BE-***.***.**", "(null)")]
        [TestCase("", "BE-***.***.**", "BE-***.***.**")]
        [TestCase("(null)", "BE-***.***.**", "(empty)")]
        [TestCase("(empty)", "********", "(empty)")]
        [TestCase("(null)", "BE-***.***.**", "(blank)")]
        [TestCase("(blank)", "********", "(blank)")]
        public void MaskToText_Valid(string expected, string mask, string value)
            => Assert.That(new MaskToText(new LiteralScalarResolver<string>(mask)).Evaluate(value)
                , Is.EqualTo(expected));
    }
}
