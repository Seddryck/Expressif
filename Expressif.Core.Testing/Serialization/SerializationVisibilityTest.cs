using System.Reflection;

namespace Expressif.Testing.Serialization;

public class SerializationVisibilityTest
{
    [TestCase("ExpressionSerializer")]
    [TestCase("FunctionSerializer")]
    [TestCase("ParameterSerializer")]
    [TestCase("PredicationSerializer")]
    [TestCase("SinglePredicationSerializer")]
    public void SourceRenderer_IsNotPublic(string typeName)
    {
        var type = typeof(ExpressionBuilder).Assembly.GetType(
            $"Expressif.Serialization.{typeName}",
            throwOnError: true);

        Assert.That(type!.IsNotPublic, Is.True);
    }

    [TestCase(typeof(ExpressionBuilder))]
    [TestCase(typeof(AbstractPredicationBuilder))]
    [TestCase(typeof(PredicationBuilder))]
    public void BuilderApi_DoesNotExposeSourceRenderers(Type builderType)
    {
        var apiConstructors = builderType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(constructor => constructor.IsPublic || constructor.IsFamily || constructor.IsFamilyOrAssembly);

        Assert.That(
            apiConstructors.SelectMany(constructor => constructor.GetParameters()),
            Has.None.Matches<System.Reflection.ParameterInfo>(
                parameter => parameter.ParameterType.Namespace == "Expressif.Serialization"));
    }
}
