using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Library.Record;
using Expressif.Library.Special;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using RecordFunction = Expressif.Library.Record.Record;
using TupleFunction = Expressif.Library.Tuple.Tuple;

namespace Expressif.Testing.Functions;

public class FunctionRegistryTest
{
    private static IImplementationRegistry CreateRegistry()
        => new FunctionRegistry(TestExpression.LibraryTypeSource);

    [Test]
    [TestCase("neutral", typeof(Neutral))]
    [TestCase("null-to-zero", typeof(NullToZero))]
    [TestCase("numeric-to-ceiling", typeof(Ceiling))]
    [TestCase("ceiling", typeof(Ceiling))]
    [TestCase("datetime-to-date", typeof(DateTimeToDate))]
    [TestCase("local-to-utc", typeof(LocalToUtc))]
    [TestCase("text-to-without-diacritics", typeof(WithoutDiacritics))]
    [TestCase("without-diacritics", typeof(WithoutDiacritics))]
    [TestCase("path-to-filename-without-extension", typeof(FilenameWithoutExtension))]
    [TestCase("filename-without-extension", typeof(FilenameWithoutExtension))]
    [TestCase("whitespaces-to-empty", typeof(WhitespacesToEmpty))]
    [TestCase("blank-to-empty", typeof(WhitespacesToEmpty))]
    [TestCase("field", typeof(Field))]
    [TestCase("record", typeof(RecordFunction))]
    [TestCase("tuple", typeof(TupleFunction))]
    public void Execute_FunctionName_Valid(string value, Type expected)
            => Assert.That(CreateRegistry().Resolve(value), Is.EqualTo(expected));

    [Test]
    [TestCase("null-to-zero")]
    [TestCase("Null-To-Zero")]
    [TestCase("NULL-To-Zero")]
    [TestCase("null - to - zero")]
    public void Execute_FunctionNameVariations_Valid(string value)
        => Assert.That(CreateRegistry().Resolve(value), Is.EqualTo(typeof(NullToZero)));

    [Test]
    [TestCase("foo")]
    [TestCase("foo-to-bar")]
    [TestCase("foo - to - bar")]
    public void Execute_FunctionName_Invalid(string value)
        => Assert.That(() => CreateRegistry().Resolve(value), Throws.TypeOf<NotImplementedFunctionException>());

    [Test]
    public void TryResolve_FunctionName_ReturnsWhetherFunctionExists()
    {
        var registry = CreateRegistry();

        Assert.Multiple(() =>
        {
            Assert.That(registry.TryResolve("ceiling", out var type), Is.True);
            Assert.That(type, Is.EqualTo(typeof(Ceiling)));
            Assert.That(registry.TryResolve("foo", out _), Is.False);
        });
    }
}
