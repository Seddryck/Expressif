﻿using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values.Casters;

public class TypeCheckerTest
{
    [Test]
    [TestCase(typeof(Byte))]
    [TestCase(typeof(SByte))]
    [TestCase(typeof(Int16))]
    [TestCase(typeof(Int32))]
    [TestCase(typeof(Int64))]
#if NET7_0_OR_GREATER
    [TestCase(typeof(Int128))]
#endif
    [TestCase(typeof(UInt16))]
    [TestCase(typeof(UInt32))]
    [TestCase(typeof(UInt64))]
#if NET7_0_OR_GREATER
    [TestCase(typeof(UInt128))]
    [TestCase(typeof(Half))]
#endif
    [TestCase(typeof(Single))]
    [TestCase(typeof(Double))]
    [TestCase(typeof(Decimal))]
    public void IsNumericType_Type_Valid(Type type)
       => Assert.That(TypeChecker.IsNumericType(Activator.CreateInstance(type)!), Is.True);

    [Test]
    [TestCase(typeof(string))]
    [TestCase(typeof(bool))]
    [TestCase(typeof(DateTime))]
    [TestCase(typeof(DateOnly))]
    [TestCase(typeof(TimeOnly))]
    [TestCase(typeof(DateTimeOffset))]
    [TestCase(typeof(TimeSpan))]
    public void IsNumericType_Type_Invalid(Type type)
       => Assert.That(TypeChecker.IsNumericType(type), Is.False);
}
