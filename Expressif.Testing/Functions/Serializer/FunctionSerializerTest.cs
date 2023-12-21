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
    public class FunctionSerializerTest
    {
        [Test]
        public void Serialize_NoParameter_NoParenthesis()
        {
            var function = new Function("Lower", []);
            Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("lower"));
        }

        [Test]
        public void Serialize_NoParameter_NoParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var function = new Function("Lower", []);
            var serializer = new FunctionSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(function);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<IParameter>()), Times.Never);
        }

        [Test]
        public void Serialize_WithSingleParameter_Parenthesis()
        {
            var function = new Function("FirstChars", [new LiteralParameter("5")]);
            Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("first-chars(5)"));
        }

        [Test]
        public void Serialize_WithSingleParameter_OneParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var function = new Function("FirstChars", [new LiteralParameter("5")]);
            var serializer = new FunctionSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(function);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Once);
        }

        [Test]
        public void Serialize_MultipleParameter_ParenthesisAndComas()
        {
            var function = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
            Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("pad-right(7, *)"));
        }

        [Test]
        public void Serialize_MultipleParameter_MultipleParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var function = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
            var serializer = new FunctionSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(function);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Exactly(2));
        }
    }
}
