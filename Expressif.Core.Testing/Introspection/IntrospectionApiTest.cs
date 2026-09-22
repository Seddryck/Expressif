using System.Reflection;
using Expressif.Discovery;
using Expressif.Introspection;
using Expressif.Library.IO;
using Expressif.Library.Text;
using ReflectionParameterInfo = System.Reflection.ParameterInfo;

namespace Expressif.Testing.Introspection;

public class IntrospectionApiTest
{
    [TestCase("Expressif.Introspection.BaseIntrospector")]
    [TestCase("Expressif.Introspection.DocumentationExtensions")]
    [TestCase("Expressif.Introspection.CtorInfo")]
    [TestCase("Expressif.Introspection.ParamInfo")]
    [TestCase("Expressif.Introspection.IntrospectionOptions")]
    [TestCase("Expressif.Introspection.ParameterIntrospectionKey")]
    [TestCase("Expressif.Introspection.CoercionIntrospector")]
    public void InfrastructureType_IsNotPublic(string typeName)
    {
        var type = typeof(FunctionIntrospector).Assembly.GetType(typeName);

        Assert.That(type, Is.Not.Null);
        Assert.That(type!.IsPublic, Is.False);
    }

    [TestCase(typeof(FunctionIntrospector))]
    [TestCase(typeof(PredicateIntrospector))]
    public void Introspector_PublicSurfaceIsSealedAndOnlyDescribes(Type type)
    {
        Assert.Multiple(() =>
        {
            Assert.That(type.IsSealed, Is.True);
            Assert.That(
                type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(method => method.Name),
                Is.EqualTo(new[] { nameof(FunctionIntrospector.Describe) }));
            Assert.That(
                type.GetConstructors().SelectMany(constructor => constructor.GetParameters()),
                Has.All.Matches<ReflectionParameterInfo>(parameter =>
                    parameter.ParameterType == typeof(Assembly[])
                    || parameter.ParameterType == typeof(ITypeSource)));
        });
    }

    [TestCase(typeof(FunctionInfo))]
    [TestCase(typeof(PredicateInfo))]
    [TestCase(typeof(Expressif.Introspection.ParameterInfo))]
    [TestCase(typeof(FunctionAliasLifecycleInfo))]
    [TestCase(typeof(TupleBindingInfo))]
    public void ReadModel_IsSealedImmutableAndCannotBeConstructedPublicly(Type type)
    {
        Assert.Multiple(() =>
        {
            Assert.That(type.IsSealed, Is.True);
            Assert.That(type.GetConstructors(), Is.Empty);
            Assert.That(
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public),
                Has.All.Matches<PropertyInfo>(property => property.SetMethod is null));
            Assert.That(type.GetMethod("Deconstruct", BindingFlags.Instance | BindingFlags.Public), Is.Null);
        });
    }

    [Test]
    public void TupleBindingInfo_DoesNotExposeReflectionImplementationDetails()
        => Assert.That(
            typeof(TupleBindingInfo).GetProperties().Select(property => property.PropertyType),
            Has.None.EqualTo(typeof(ConstructorInfo)));

    [Test]
    public void Describe_CustomTypeSource_ReturnsMaterializedReadOnlyMetadata()
    {
        var source = new FixedTypeSource(typeof(Filename), typeof(IsKebabCase));

        var functions = new FunctionIntrospector(source).Describe();
        var predicates = new PredicateIntrospector(source).Describe();

        Assert.Multiple(() =>
        {
            Assert.That(functions.Select(info => info.ImplementationType), Is.EqualTo(new[] { typeof(Filename) }));
            Assert.That(predicates.Select(info => info.ImplementationType), Is.EqualTo(new[] { typeof(IsKebabCase) }));
            Assert.That(() => ((IList<FunctionInfo>)functions).Add(functions[0]), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => ((IList<PredicateInfo>)predicates).Add(predicates[0]), Throws.TypeOf<NotSupportedException>());
        });
    }

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }
}
