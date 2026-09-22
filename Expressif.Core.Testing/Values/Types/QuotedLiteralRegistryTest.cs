using System.Globalization;
using Expressif.Bindings;
using Expressif.Serialization;
using Expressif.Syntax;
using Expressif.Types;
using Expressif.Values.Types;

namespace Expressif.Testing.Types;

public class QuotedLiteralRegistryTest
{
    private static readonly ITypeRegistry Types = new TypeRegistry(
    [
        Descriptor("date", typeof(DateOnly)),
        Descriptor("datetime", typeof(DateTime)),
        Descriptor("time", typeof(TimeOnly)),
        Descriptor("text", typeof(string)),
        Descriptor("numeric", typeof(decimal)),
    ]);

    [TestCase("#\"2025-12-16\"", typeof(DateOnly), "date", false)]
    [TestCase("#\"2025-12-16T14:30:00\"", typeof(DateTime), "datetime", false)]
    [TestCase("#\"14:30:00\"", typeof(TimeOnly), "time", false)]
    [TestCase("#\"2025-12-16\":date", typeof(DateOnly), "date", true)]
    [TestCase("#\"2025-12-16T14:30:00\":datetime", typeof(DateTime), "datetime", true)]
    [TestCase("#\"14:30:00\":time", typeof(TimeOnly), "time", true)]
    [TestCase("#\"2025-12-16 14:30:00\":date-time", typeof(DateTime), "datetime", true)]
    public void Binder_BuiltInQuotedLiteral_ReturnsSelectedTypedValue(
        string source, Type runtimeType, string typeName, bool isTypeExplicit)
    {
        var parameter = Bind(source, QuotedLiteralRegistry.Default);

        Assert.Multiple(() =>
        {
            Assert.That(parameter.Value, Is.TypeOf(runtimeType));
            Assert.That(parameter.LiteralType, Is.EqualTo(typeName));
            Assert.That(parameter.IsLiteralTypeExplicit, Is.EqualTo(isTypeExplicit));
        });
    }

    [Test]
    public void Parse_NoAcceptingParser_ThrowsInvalidLiteral()
    {
        var registry = new QuotedLiteralRegistry([new TestParser("text", typeof(string), _ => null)]);

        Assert.That(() => Bind("#\"rejected\"", registry), Throws.TypeOf<InvalidQuotedLiteralException>());
    }

    [Test]
    public void Parse_OneAcceptingParser_ReturnsItsValue()
    {
        var registry = new QuotedLiteralRegistry([new TestParser("text", typeof(string), value => value.ToUpperInvariant())]);

        Assert.That(Bind("#\"accepted\"", registry),
            Is.EqualTo(new LiteralParameter("ACCEPTED", "text")));
    }

    [Test]
    public void Parse_MultipleAcceptingParsers_ReportsSortedTypesRegardlessOfRegistrationOrder()
    {
        IQuotedLiteralParser date = new TestParser("date", typeof(DateOnly), _ => new DateOnly(2025, 12, 16));
        IQuotedLiteralParser text = new TestParser("text", typeof(string), value => value);
        var first = Assert.Throws<AmbiguousQuotedLiteralException>(
            () => Bind("#\"ambiguous\"", new([text, date])));
        var second = Assert.Throws<AmbiguousQuotedLiteralException>(
            () => Bind("#\"ambiguous\"", new([date, text])));

        Assert.Multiple(() =>
        {
            Assert.That(first!.Message, Is.EqualTo(second!.Message));
            Assert.That(first.Message, Does.Contain(":date, :text").And.Contain("explicit type suffix"));
        });
    }

    [Test]
    public void Parse_ExplicitSuffix_InvokesOnlySelectedParser()
    {
        var selected = new TestParser("date", typeof(DateOnly), _ => new DateOnly(2025, 12, 16));
        var other = new TestParser("text", typeof(string), _ => throw new AssertionException("Unexpected parser invocation."));
        var registry = new QuotedLiteralRegistry([other, selected]);

        Assert.That(Bind("#\"anything\":date", registry).Value, Is.EqualTo(new DateOnly(2025, 12, 16)));
    }

    [TestCase("#\"anything\":missing", typeof(UnknownExpressifTypeException))]
    [TestCase("#\"42\":numeric", typeof(UnsupportedQuotedLiteralTypeException))]
    [TestCase("#\"not-a-date\":date", typeof(InvalidQuotedLiteralException))]
    public void Parse_InvalidExplicitLiteral_ThrowsSpecificDiagnostic(string source, Type exceptionType)
        => Assert.That(() => Bind(source, QuotedLiteralRegistry.Default), Throws.TypeOf(exceptionType));

    [Test]
    public void Serialize_ExplicitType_RoundTripsWhenAnotherParserAcceptsRepresentation()
    {
        var registry = new QuotedLiteralRegistry(
            [
                new DateParser(),
                new TestParser("text", typeof(string), value => value),
            ]);
        var serializer = new ParameterSerializer(registry);
        var original = new LiteralParameter(new DateOnly(2025, 12, 16), "date", IsLiteralTypeExplicit: true);

        var serialized = serializer.Serialize(original);
        var roundTrip = Bind(serialized, registry);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("#\"2025-12-16\":date"));
            Assert.That(roundTrip, Is.EqualTo(original));
        });
    }

    [Test]
    public void Serialize_ImplicitBuiltInType_AddsSuffixWhenRepresentationIsAmbiguous()
    {
        var registry = new QuotedLiteralRegistry(
            [
                new DateParser(),
                new TestParser("text", typeof(string), value => value),
            ]);
        var serializer = new ParameterSerializer(registry);

        var serialized = serializer.Serialize(new LiteralParameter(new DateOnly(2025, 12, 16), "date"));

        Assert.That(serialized, Is.EqualTo("#\"2025-12-16\":date"));
    }

    private static LiteralParameter Bind(string source, QuotedLiteralRegistry registry)
        => (LiteralParameter)new ExpressifBinder(
            [],
            new FunctionBinderRegistry(Array.Empty<IFunctionBinder>()),
            Types,
            quotedLiteralRegistry: registry).BindParameter(ExpressionParser.Parse(source));

    private static TypeDescriptor Descriptor(string name, Type runtimeType)
        => new(name, string.Empty, null, null, new Dictionary<string, string>(), runtimeType);

    private sealed class TestParser(
        string typeName,
        Type runtimeType,
        Func<string, object?> parse) : IQuotedLiteralParser
    {
        public string TypeName => typeName;
        public Type RuntimeType => runtimeType;
        public bool TryParse(string representation, out object? value)
        {
            value = parse(representation);
            return value is not null;
        }
        public string Format(object value) => Convert.ToString(value, CultureInfo.InvariantCulture)!;
    }

    private sealed class DateParser : IQuotedLiteralParser
    {
        public string TypeName => "date";
        public Type RuntimeType => typeof(DateOnly);
        public bool TryParse(string representation, out object? value)
        {
            var accepted = DateOnly.TryParseExact(representation, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date);
            value = accepted ? date : null;
            return accepted;
        }
        public string Format(object value)
            => ((DateOnly)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
