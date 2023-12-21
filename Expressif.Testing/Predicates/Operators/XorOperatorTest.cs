﻿using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Predicates.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Operators;

public class XorOperatorTest
{
    [Test]
    [TestCase(true, true, false)]
    [TestCase(true, false, true)]
    [TestCase(false, false, false)]
    [TestCase(false, true, true)]
    public void Evaluate_Value_Success(bool state, bool value, bool expected)
    {
        var left = new Mock<IPredicate>();
        left.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(state);
        var right = new Mock<IPredicate>();
        right.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(value);
        var @operator = new XorOperator(left.Object, right.Object);

        Assert.That(@operator.Evaluate("my value"), Is.EqualTo(expected));
    }

    [Test]
    public void Evaluate_LeftIsTrue_EvaluateRightPredicate()
    {
        var left = new Mock<IPredicate>();
        left.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(true);
        var right = new Mock<IPredicate>();
        right.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(false);

        var @operator = new XorOperator(left.Object, right.Object);
        @operator.Evaluate("my value");

        left.Verify(x => x.Evaluate("my value"), Times.Once());
        right.Verify(x => x.Evaluate("my value"), Times.Once());
    }

    [Test]
    public void Evaluate_LeftIsFalse_EvaluateRightPredicate()
    {
        var left = new Mock<IPredicate>();
        left.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(false);
        var right = new Mock<IPredicate>();
        right.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(false);

        var @operator = new XorOperator(left.Object, right.Object);
        @operator.Evaluate("my value");

        left.Verify(x => x.Evaluate("my value"), Times.Once());
        right.Verify(x => x.Evaluate("my value"), Times.Once());
    }
}
