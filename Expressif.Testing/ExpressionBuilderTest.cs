using Expressif.Functions.Text;
using Expressif.Parsers;
using Expressif.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing;

public class ExpressionBuilderTest
{
    [Test]
    public void Build_NoPredicate_ThrowException()
        => Assert.Throws<InvalidOperationException>(() => new ExpressionBuilder().Build());

    [Test]
    public void Chain_WithoutParameter_CorrectlyEvaluate()
    {
        var builder = new ExpressionBuilder().Chain<Lower>();
        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla"));
    }

    [Test]
    public void Chain_WithParameter_CorrectlyEvaluate()
    {
        var builder = new ExpressionBuilder().Chain<FirstChars>(5);
        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikol"));
    }

    [Test]
    public void Chain_WithParameters_CorrectlyEvaluate()
    {
        var builder = new ExpressionBuilder().Chain<PadRight>(15, '*');
        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikola Tesla***"));
    }

    [Test]
    public void Chain_MultipleWithoutParameters_CorrectlyEvaluate()
    {
        var builder = new ExpressionBuilder()
            .Chain<Lower>()
            .Chain<Length>();
        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo(12));
    }

    [Test]
    public void Chain_Multiple_CorrectlyEvaluate()
    {
        var builder = new ExpressionBuilder()
            .Chain<Lower>()
            .Chain<FirstChars>(5)
            .Chain<PadRight>(7, '*');
        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
    }

    [Test]
    public void Chain_MultipleWithContext_CorrectlyEvaluate()
    {
        var context = new Context();
        var builder = new ExpressionBuilder(context)
            .Chain<Lower>()
            .Chain<PadRight>(ctx => ctx.Variables["myVar"], ctx => ctx.CurrentObject[1]);
        var expression = builder.Build();

        context.Variables.Add<int>("myVar", 15);
        context.CurrentObject.Set(new List<char>() { '-', '*', ' ' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla***"));

        context.Variables.Set("myVar", 16);
        context.CurrentObject.Set(new List<char>() { '*', '+' });
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla++++"));
    }

    [Test]
    public void Chain_NotGeneric_CorrectlyEvaluate()
    {
        var builder = new ExpressionBuilder()
            .Chain(typeof(Lower))
            .Chain(typeof(FirstChars), 5)
            .Chain(typeof(PadRight), 7, '*');
        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
    }

    [Test]
    public void Chain_SubExpression_CorrectlyEvaluate()
    {
        var subExpressionBuilder = new ExpressionBuilder()
            .Chain<FirstChars>(5)
            .Chain<PadRight>(7, '*');

        var builder = new ExpressionBuilder()
            .Chain<Lower>()
            .Chain(subExpressionBuilder)
            .Chain<Upper>();

        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("NIKOL**"));
    }

    [Test]
    public void Serialize_WithParameters_CorrectlyEvaluate()
    {
        var internalSerializer = new Mock<FunctionSerializer>();
        var serializer = new ExpressionSerializer(internalSerializer.Object);
        var builder = new ExpressionBuilder(serializer: serializer)
            .Chain<Lower>()
            .Chain<FirstChars>(5)
            .Chain<PadRight>(7, '*');
        var str = builder.Serialize();
        internalSerializer.Verify(x=>x.Serialize(It.IsAny<Function>(), ref It.Ref<StringBuilder>.IsAny), Times.Exactly(3));
    }

    [Test]
    public void Serialize_WithParameters_CorrectlySerialized()
    {
        var builder = new ExpressionBuilder()
            .Chain<Lower>()
            .Chain<FirstChars>(5)
            .Chain<PadRight>(7, '*');
        var str = builder.Serialize();
        Assert.That(str, Is.EqualTo("lower | first-chars(5) | pad-right(7, *)"));
    }

    [Test]
    public void Serialize_NoPredicate_ThrowException()
        => Assert.Throws<InvalidOperationException>(() => new ExpressionBuilder().Serialize());
}
