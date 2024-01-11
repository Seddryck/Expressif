using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public void Cast_TypedCasterToDecimal_Correct()
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
    public void Cast_TypedCasterToDateTime_Correct()
        => Assert.Multiple(() =>
        {
            Assert.That(new Caster().Cast<DateTime>("2023-10-18"), Is.EqualTo(new DateTime(2023, 10, 18)));
            Assert.That(new Caster().Cast<DateTime>("2023-10-18 07:10:45"), Is.EqualTo(new DateTime(2023, 10, 18, 7, 10, 45)));
        });
}
