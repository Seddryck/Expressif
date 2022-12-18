using Expressif.Predicates.Serializer;
using Expressif.Predicates;
using Expressif.Predicates.Numeric;
using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Predicates.Combination;

namespace Expressif.Testing.Predicates.Serializer
{
    public class PredicationSerializerTest
    {
        [Test]
        public void Serialize_SingleMember_NoPipe()
        {
            var Predication = new PredicationBuilder();
            Predication.Pile.Enqueue(new PredicationMember(typeof(Even), Array.Empty<object>()));
            Assert.That(new PredicationSerializer().Serialize(Predication), Is.EqualTo("even"));
        }

        [Test]
        public void Serialize_SingleParameter_SinglePredicationMemberSerializerCall()
        {
            var internalSerializer = new Mock<PredicationMemberSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<PredicationMember>())).Returns("exp");

            var Predication = new PredicationBuilder();
            Predication.Pile.Enqueue(new PredicationMember(typeof(Even), Array.Empty<object>()));
            var serializer = new PredicationSerializer(predicationMemberSerializer: internalSerializer.Object);
            serializer.Serialize(Predication);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<PredicationMember>()), Times.Once);
        }

        [Test]
        public void Serialize_SingleMember_QueueNotEmpty()
        {
            var Predication = new PredicationBuilder();
            Predication.Pile.Enqueue(new PredicationMember(typeof(Even), Array.Empty<object>()));
            new PredicationSerializer().Serialize(Predication);
            Assert.That(Predication.Pile, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void Serialize_MultipleMembers_WithPipe()
        {
            var Predication = new PredicationBuilder();
            Predication.Pile.Enqueue(new PredicationMember(typeof(Even), Array.Empty<object>()));
            Predication.Pile.Enqueue(new PredicationMember(typeof(OrOperator), typeof(GreaterThan), new object[] { 5 }));
            Predication.Pile.Enqueue(new PredicationMember(typeof(AndOperator), typeof(Modulo), new object[] { 7, 3 }));
            Assert.That(new PredicationSerializer().Serialize(Predication)
                , Is.EqualTo("even |OR greater-than(5) |AND modulo(7, 3)"));
        }

        [Test]
        public void Serialize_MultipleMembers_MultiplePredicationMemberSerializerCall()
        {
            var internalSerializer = new Mock<PredicationMemberSerializer>();
            internalSerializer.Setup(x => x.Serialize(It.IsAny<PredicationMember>())).Returns("exp");

            var Predication = new PredicationBuilder();
            Predication.Pile.Enqueue(new PredicationMember(typeof(Even), Array.Empty<object>()));
            Predication.Pile.Enqueue(new PredicationMember(typeof(OrOperator), typeof(GreaterThan), new object[] { 5 }));
            Predication.Pile.Enqueue(new PredicationMember(typeof(AndOperator), typeof(Modulo), new object[] { 7, 3 }));
            var serializer = new PredicationSerializer(predicationMemberSerializer: internalSerializer.Object);
            serializer.Serialize(Predication);

            internalSerializer.Verify(x => x.Serialize(It.IsAny<PredicationMember>()), Times.Exactly(3));
        }

        [Test]
        public void Serialize_WithSubPredication_WithPipe()
        {
            var subPredication = new PredicationBuilder();
            subPredication.Pile.Enqueue(new PredicationMember(typeof(GreaterThan), new object[] { 5 }));
            subPredication.Pile.Enqueue(new PredicationMember(typeof(OrOperator), typeof(Modulo), new object[] { 7, 3 }));

            var Predication = new PredicationBuilder();
            Predication.Pile.Enqueue(new PredicationMember(typeof(Even), Array.Empty<object>()));
            Predication.Pile.Enqueue(new SubPredicationMember(typeof(AndOperator), subPredication));
            Predication.Pile.Enqueue(new PredicationMember(typeof(OrOperator), typeof(ZeroOrNull), Array.Empty<object>()));

            Assert.That(new PredicationSerializer().Serialize(Predication)
                , Is.EqualTo("even |AND { greater-than(5) |OR modulo(7, 3) } |OR zero-or-null"));
        }
    }
}
