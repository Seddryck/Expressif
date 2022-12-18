using Expressif.Functions.Serializer;
using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Serializer
{
    public class ParameterSerializerTest
    {
        [Test]
        [TestCase("foo")]
        [TestCase("foo_bar")]
        [TestCase("123")]
        [TestCase("123.125")]
        public void Serialize_LiteralParameter_Unquoted(string value)
            => Assert.That(new ParameterSerializer().Serialize(new LiteralParameter(value)), Is.EqualTo(value));

        [Test]
        [TestCase("foo@bar")]
        [TestCase("{foo_bar}")]
        [TestCase("(123)")]
        [TestCase("123,125")]
        public void Serialize_LiteralParameter_Quoted(string value)
            => Assert.That(new ParameterSerializer().Serialize(new LiteralParameter(value)), Is.EqualTo($"\"{value}\""));

        [Test]
        [TestCase("foo")]
        [TestCase("foo123")]
        public void Serialize_VariableParameter_Arobas(string value)
            => Assert.That(new ParameterSerializer().Serialize(new VariableParameter(value)), Is.EqualTo($"@{value}"));

        [Test]
        [TestCase("foo")]
        [TestCase("foo123")]
        public void Serialize_ObjectPropertyParameter_Brakets(string value)
            => Assert.That(new ParameterSerializer().Serialize(new ObjectPropertyParameter(value)), Is.EqualTo($"[{value}]"));

        [Test]
        [TestCase(0)]
        [TestCase(10)]
        public void Serialize_ObjectIndexParameter_Brakets(int value)
            => Assert.That(new ParameterSerializer().Serialize(new ObjectIndexParameter(value)), Is.EqualTo($"#{value}"));
    }
}
