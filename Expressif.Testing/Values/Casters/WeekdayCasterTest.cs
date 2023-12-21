using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values.Casters;

internal class WeekdayCasterTest
{
    public WeekdayCasterTest()
    {
        TypeDescriptor.AddAttributes(
            typeof(DateOnly)
            , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
        );
    }

    [Test]
    [TestCase("Saturday", typeof(string), "Saturday")]
    [TestCase("  sAtUrDaY  ", typeof(string), "Saturday")]
    [TestCase("5", typeof(int), "Saturday")]
    [TestCase("Saturday", typeof(Weekday), "Saturday")]
    public void TryCast_Success(string text, Type type, string expected)
    {
        var obj = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(text)!;
        Assert.Multiple(() =>
        {
            Assert.That(new WeekdayCaster().TryCast(obj, out var value), Is.True);
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Is.TypeOf<Weekday>());
            Assert.That(value!.Name, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(Weekdays.Saturday));
        });
    }

    [Test]
    [TestCase("foo", typeof(string))]
    [TestCase("Anyday", typeof(string))]
    [TestCase("16", typeof(int))]
    [TestCase("16.125", typeof(decimal))]
    [TestCase("2022-12", typeof(YearMonth))]
    [TestCase("2022-12-28", typeof(DateOnly))]
    [TestCase("2022-12-28 16:45:00", typeof(DateTime))]
    public void TryCast_Failure(string text, Type type)
    {
        var obj = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(text)!;
        Assert.Multiple(() =>
        {
            Assert.That(new WeekdayCaster().TryCast(obj, out var value), Is.False);
            Assert.That(value, Is.Null);
        });
    }
}
