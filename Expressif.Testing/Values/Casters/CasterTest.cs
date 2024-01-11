using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Testing.Values.Casters;
public class CasterTest
{
    [Test]
    public void Cast_NullToNullableType_Null()
        => Assert.That(new Caster().Cast<string>(null), Is.Null);

    [Test]
    public void Cast_NullToPrimitive_Null()
    => Assert.That(new Caster().Cast<int>(null), Is.Zero);

    [Test]
    public void Cast_DBNullToNullableType_Null()
    => Assert.That(new Caster().Cast<string>(DBNull.Value), Is.Null);

    [Test]
    public void Cast_DBNullToPrimitive_Null()
        => Assert.That(new Caster().Cast<int>(DBNull.Value), Is.Zero);

    [Test]
    public void Cast_TypedToNullableType_Itself()
    => Assert.That(new Caster().Cast<string>("abc"), Is.EqualTo("abc"));

    [Test]
    public void Cast_TypedToPrimitive_Itself()
        => Assert.That(new Caster().Cast<int>(123), Is.EqualTo(123));

    [Test]
    public void Cast_ImplicitConvert_Correct()
        => Assert.That(new Caster().Cast<DateTime>("2023-01-01"), Is.EqualTo(new DateTime(2023,1,1)));

    [Test]
    public void Cast_TypedCasterToNumeric_Correct()
        => Assert.Multiple(() =>
        {
            Assert.That(new Caster().Cast<decimal>(10), Is.EqualTo(10m));
            Assert.That(new Caster().Cast<decimal>(10L), Is.EqualTo(10m));
            Assert.That(new Caster().Cast<decimal>(10f), Is.EqualTo(10m));
            Assert.That(new Caster().Cast<decimal>(10d), Is.EqualTo(10m));
            Assert.That(new Caster().Cast<decimal>("10"), Is.EqualTo(10m));
            Assert.That(new Caster().Cast<decimal>("10.0"), Is.EqualTo(10m));
            Assert.That(new Caster().Cast<decimal>(true), Is.EqualTo(1m));
        });

    [Test]
    public void Cast_TypedCasterToBoolean_Correct()
         => Assert.Multiple(() =>
         {
             Assert.That(new Caster().Cast<bool>("true"), Is.EqualTo(true));
             Assert.That(new Caster().Cast<bool>(1), Is.EqualTo(true));
         });

    [Test]
    public void Cast_TypedCasterToDateTime_Correct()
        => Assert.Multiple(() =>
        {
            Assert.That(new Caster().Cast<DateTime>("2023-10-18"), Is.EqualTo(new DateTime(2023, 10, 18)));
            Assert.That(new Caster().Cast<DateTime>("2023-10-18 07:10:45"), Is.EqualTo(new DateTime(2023, 10, 18, 7, 10, 45)));
        });

    [Test]
    public void Cast_TypedCasterToYearMonth_Correct()
        => Assert.That(new Caster().Cast<YearMonth>("2023-10"), Is.EqualTo(new YearMonth(2023, 10)));

    [Test]
    public void Cast_TypedCasterToText_Correct()
        => Assert.Multiple(() =>
        {
            Assert.That(new Caster().Cast<string>(new DateTime(2023, 10, 18)), Is.EqualTo("2023-10-18 00:00:00"));
            Assert.That(new Caster().Cast<string>(10f), Is.EqualTo("10"));
        });

    [Test]
    public void Cast_NotCasterableToTimeSpan_Throws()
        => Assert.Multiple(() =>
        {
            Assert.That(() => new Caster().Cast<TimeSpan>(10f), Throws.ArgumentException);
        });
}
