using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;

namespace Expressif.Testing.Functions.Temporal;
public class TimeZoneFunctionsTest
{
    [Test]
    public void SetToUtc_Integer_Valid()
    {
        var value = new SetToUtc().Evaluate(new DateTime(2023, 05, 01, 15, 0, 0));
        Assert.That(value, Is.TypeOf<DateTime>());
        Assert.Multiple(() =>
        {
            Assert.That(((DateTime)value).Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(((DateTime)value), Is.EqualTo(new DateTime(2023, 05, 01, 15, 0, 0)));
        });
    }

    [Test]
    public void SetToLocal_Integer_Valid()
    {
        var value = new SetToLocal().Evaluate(new DateTime(2023, 05, 01, 15, 0, 0));
        Assert.That(value, Is.TypeOf<DateTime>());
        Assert.Multiple(() =>
        {
            Assert.That(((DateTime)value).Kind, Is.EqualTo(DateTimeKind.Local));
            Assert.That(((DateTime)value), Is.EqualTo(new DateTime(2023, 05, 01, 15, 0, 0)));
        });
    }
}
