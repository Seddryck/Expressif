using Expressif.Parsers;
using Expressif.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Serializers
{
    public class ExpressionSerializerTest
    {
        [Test]
        public void Serialize_SingleMember_NoPipe()
        {
            var expression = new Function("Lower", []);
            Assert.That(new ExpressionSerializer().Serialize([expression]), Is.EqualTo("lower"));
        }

        [Test]
        public void Serialize_SingleParameter_SingleExpressionMemberSerializerCall()
        {
            var internalSerializer = new Mock<FunctionSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<Function>())).Returns("exp");

            var expression = new Function("Lower", []); var serializer = new ExpressionSerializer(internalSerializer.Object);
            serializer.Serialize([expression]);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<Function>(), ref It.Ref<StringBuilder>.IsAny), Times.Once);
        }

        [Test]
        public void Serialize_MultipleMembers_WithPipe()
        {
            var lowerExpression = new Function("Lower", []);
            var firstCharsExpression = new Function("FirstChars", [new LiteralParameter("5")]);
            var PadRightExpression = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
            Assert.That(new ExpressionSerializer().Serialize([lowerExpression, firstCharsExpression, PadRightExpression])
                , Is.EqualTo("lower | first-chars(5) | pad-right(7, *)"));
        }

        [Test]
        public void Serialize_MultipleMembers_MultipleExpressionMemberSerializerCall()
        {
            var internalSerializer = new Mock<FunctionSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<Function>())).Returns("exp");

            var lowerExpression = new Function("Lower", []);
            var firstCharsExpression = new Function("FirstChars", [new LiteralParameter("5")]);
            var PadRightExpression = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
            var serializer = new ExpressionSerializer(internalSerializer.Object);
            serializer.Serialize([lowerExpression, firstCharsExpression, PadRightExpression]);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<Function>(), ref It.Ref<StringBuilder>.IsAny), Times.Exactly(3));
        }

        [Test]
        [Ignore("Limited added-value to manage subexpression")]
        public void Serialize_WithSubExpression_WithPipe()
        {
            var lowerExpression = new Function("Lower", []);
            var firstCharsExpression = new Function("FirstChars", [new LiteralParameter("5")]);
            var PadRightExpression = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
            var upperExpression = new Function("Upper", []);

            var subExpression = new Expressif.Parsers.Expression([firstCharsExpression, PadRightExpression]);

            //var expression = new Expressif.Parsers.Expression([lowerExpression, subExpression, upperExpression]);

            //Assert.That(new ExpressionSerializer().Serialize(expression)
            //    , Is.EqualTo("lower | { first-chars(5) | pad-right(7, *) } | upper"));
        }
    }
}
