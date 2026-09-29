using Expressif.Functions;
using Expressif.Functions.Coercions;
using Expressif.Values.Types;

namespace Expressif.Testing.Introspection;

public sealed class IntrospectionOptionsTest
{
    [Test]
    public void MinimalConstructorCreatesEmptyVocabularyOverrides()
    {
        var types = Mock.Of<ITypeRegistry>();
        var coercions = Mock.Of<ICoercionRegistry>();

        var options = new IntrospectionOptions(types, coercions);

        Assert.Multiple(() =>
        {
            Assert.That(options.Types, Is.SameAs(types));
            Assert.That(options.Coercions, Is.SameAs(coercions));
            Assert.That(options.ParameterTypes, Is.Empty);
            Assert.That(options.ParameterNames, Is.Empty);
            Assert.That(options.VariadicParameters, Is.Empty);
            Assert.That(options.UntypedReasons, Is.Empty);
            Assert.That(options.OutputOverrides, Is.Empty);
            Assert.That(options.TupleBindingSignatures(typeof(SampleFunction)), Is.Empty);
        });
    }

    [Test]
    public void ParameterIntrospectionKeyUsesDeclaringTypeAndParameterName()
    {
        var key = new ParameterIntrospectionKey(typeof(SampleFunction), "value");

        Assert.That(key, Is.EqualTo(new ParameterIntrospectionKey(typeof(SampleFunction), "value")));
        Assert.That(key, Is.Not.EqualTo(new ParameterIntrospectionKey(typeof(SampleFunction), "other")));
    }

    private sealed class SampleFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
