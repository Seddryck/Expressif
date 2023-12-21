using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Functions.IO;
using Expressif.Functions.Numeric;
using Expressif.Functions.Special;
using Expressif.Functions.Temporal;
using Expressif.Functions.Text;

namespace Expressif.Testing.Functions;
public class PredicateTypeMapperTest
{
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
    public void Execute_FunctionName_Valid(string value, Type expected)
            => Assert.That(new FunctionTypeMapper().Execute(value), Is.EqualTo(expected));

    [Test]
    [TestCase("null-to-zero")]
    [TestCase("Null-To-Zero")]
    [TestCase("NULL-To-Zero")]
    [TestCase("null - to - zero")]
    public void Execute_FunctionNameVariations_Valid(string value)
        => Assert.That(new FunctionTypeMapper().Execute(value), Is.EqualTo(typeof(NullToZero)));

    [Test]
    [TestCase("foo")]
    [TestCase("foo-to-bar")]
    [TestCase("foo - to - bar")]
    public void Execute_FunctionName_Invalid(string value)
        => Assert.That(() => new FunctionTypeMapper().Execute(value), Throws.TypeOf<NotImplementedFunctionException>());
}
