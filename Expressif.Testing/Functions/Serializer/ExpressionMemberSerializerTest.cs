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
    public class ExpressionMemberSerializerTest
    {
        [Test]
        public void Serialize_NoParameter_NoParenthesis()
        {
            var member = new ExpressionMember(typeof(Lower), Array.Empty<object>());
            Assert.That(new ExpressionMemberSerializer().Serialize(member), Is.EqualTo("lower"));
        }

        [Test]
        public void Serialize_NoParameter_NoParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var member = new ExpressionMember(typeof(Lower), Array.Empty<object>());
            var serializer = new ExpressionMemberSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(member);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<IParameter>()), Times.Never);
        }

        [Test]
        public void Serialize_WithSingleParameter_Parenthesis()
        {
            var member = new ExpressionMember(typeof(FirstChars), new object[] { 5 });
            Assert.That(new ExpressionMemberSerializer().Serialize(member), Is.EqualTo("first-chars(5)"));
        }

        [Test]
        public void Serialize_WithSingleParameter_OneParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var member = new ExpressionMember(typeof(FirstChars), new object[] { 5 });
            var serializer = new ExpressionMemberSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(member);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Once);
        }

        [Test]
        public void Serialize_MultipleParameter_ParenthesisAndComas()
        {
            var member = new ExpressionMember(typeof(PadRight), new object[] { 7, "*" });
            Assert.That(new ExpressionMemberSerializer().Serialize(member), Is.EqualTo("pad-right(7, *)"));
        }

        [Test]
        public void Serialize_MultipleParameter_MultipleParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var member = new ExpressionMember(typeof(PadRight), new object[] { 7, "*" });
            var serializer = new ExpressionMemberSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(member);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Exactly(2));
        }
    }
}
