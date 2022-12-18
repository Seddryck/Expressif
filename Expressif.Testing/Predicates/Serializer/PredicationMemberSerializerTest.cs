using Expressif.Predicates.Serializer;
using Expressif.Predicates;
using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Predicates.Text;
using Expressif.Functions.Text;
using Expressif.Predicates.Numeric;
using Expressif.Functions.Serializer;

namespace Expressif.Testing.Predicates.Serializer
{
    public class PredicationMemberSerializerTest
    {
        [Test]
        public void Serialize_NoParameter_NoParenthesis()
        {
            var member = new PredicationMember(typeof(Even), Array.Empty<object>());
            Assert.That(new PredicationMemberSerializer().Serialize(member), Is.EqualTo("even"));
        }

        [Test]
        public void Serialize_NoParameter_NoParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var member = new PredicationMember(typeof(Even), Array.Empty<object>());
            var serializer = new PredicationMemberSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(member);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<IParameter>()), Times.Never);
        }

        [Test]
        public void Serialize_WithSingleParameter_Parenthesis()
        {
            var member = new PredicationMember(typeof(GreaterThan), new object[] { 5 });
            Assert.That(new PredicationMemberSerializer().Serialize(member), Is.EqualTo("greater-than(5)"));
        }

        [Test]
        public void Serialize_WithSingleParameter_OneParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var member = new PredicationMember(typeof(GreaterThan), new object[] { 5 });
            var serializer = new PredicationMemberSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(member);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Once);
        }

        [Test]
        public void Serialize_MultipleParameter_ParenthesisAndComas()
        {
            var member = new PredicationMember(typeof(Modulo), new object[] { 7, 3 });
            Assert.That(new PredicationMemberSerializer().Serialize(member), Is.EqualTo("modulo(7, 3)"));
        }

        [Test]
        public void Serialize_MultipleParameter_MultipleParameterSerializerCall()
        {
            var internalSerializer = new Mock<ParameterSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<IParameter>())).Returns("param");

            var member = new PredicationMember(typeof(Modulo), new object[] { 7, 3 });
            var serializer = new PredicationMemberSerializer(parameterSerializer: internalSerializer.Object);
            serializer.Serialize(member);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<LiteralParameter>()), Times.Exactly(2));
        }
    }
}
