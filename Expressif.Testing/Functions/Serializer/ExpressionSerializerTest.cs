using Expressif.Functions.Serializer;
using Expressif.Functions;
using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Predicates.Text;
using Expressif.Functions.Text;

namespace Expressif.Testing.Functions.Serializer
{
    public class ExpressionSerializerTest
    {
        [Test]
        public void Serialize_SingleMember_NoPipe()
        {
            var expression = new ExpressionBuilder();
            expression.Pile.Enqueue(new ExpressionMember(typeof(Lower), Array.Empty<object>()));
            Assert.That(new ExpressionSerializer().Serialize(expression), Is.EqualTo("lower"));
        }

        [Test]
        public void Serialize_SingleParameter_SingleExpressionMemberSerializerCall()
        {
            var internalSerializer = new Mock<ExpressionMemberSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<ExpressionMember>())).Returns("exp");

            var expression = new ExpressionBuilder();
            expression.Pile.Enqueue(new ExpressionMember(typeof(Lower), Array.Empty<object>()));
            var serializer = new ExpressionSerializer(expressionMemberSerializer: internalSerializer.Object);
            serializer.Serialize(expression);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<ExpressionMember>()), Times.Once);
        }

        [Test]
        public void Serialize_SingleMember_QueueNotEmpty()
        {
            var expression = new ExpressionBuilder();
            expression.Pile.Enqueue(new ExpressionMember(typeof(Lower), Array.Empty<object>()));
            new ExpressionSerializer().Serialize(expression);
            Assert.That(expression.Pile, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void Serialize_MultipleMembers_WithPipe()
        {
            var expression = new ExpressionBuilder();
            expression.Pile.Enqueue(new ExpressionMember(typeof(Lower), Array.Empty<object>()));
            expression.Pile.Enqueue(new ExpressionMember(typeof(FirstChars), new object[] { 5 }));
            expression.Pile.Enqueue(new ExpressionMember(typeof(PadRight), new object[] { 7, "*" }));
            Assert.That(new ExpressionSerializer().Serialize(expression)
                , Is.EqualTo("lower | first-chars(5) | pad-right(7, *)"));
        }

        [Test]
        public void Serialize_MultipleMembers_MultipleExpressionMemberSerializerCall()
        {
            var internalSerializer = new Mock<ExpressionMemberSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<ExpressionMember>())).Returns("exp");

            var expression = new ExpressionBuilder();
            expression.Pile.Enqueue(new ExpressionMember(typeof(Lower), Array.Empty<object>()));
            expression.Pile.Enqueue(new ExpressionMember(typeof(FirstChars), new object[] { 5 }));
            expression.Pile.Enqueue(new ExpressionMember(typeof(PadRight), new object[] { 7, "*" }));
            var serializer = new ExpressionSerializer(expressionMemberSerializer: internalSerializer.Object);
            serializer.Serialize(expression);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<ExpressionMember>()), Times.Exactly(3));
        }

        [Test]
        public void Serialize_WithSubExpression_WithPipe()
        {
            var subExpression = new ExpressionBuilder();
            subExpression.Pile.Enqueue(new ExpressionMember(typeof(FirstChars), new object[] { 5 }));
            subExpression.Pile.Enqueue(new ExpressionMember(typeof(PadRight), new object[] { 7, "*" }));

            var expression = new ExpressionBuilder();
            expression.Pile.Enqueue(new ExpressionMember(typeof(Lower), Array.Empty<object>()));
            expression.Pile.Enqueue(subExpression);
            expression.Pile.Enqueue(new ExpressionMember(typeof(Upper), Array.Empty<object>()));

            Assert.That(new ExpressionSerializer().Serialize(expression)
                , Is.EqualTo("lower | { first-chars(5) | pad-right(7, *) } | upper"));
        }
    }
}
