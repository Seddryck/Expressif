using Expressif.Values;
using Expressif.Values.Special;
using Expressif.Values.Types;
using System.Reflection;

namespace Expressif.Testing.Values;

[TestFixture]
public class ValuesApiTest
{
    private static readonly Assembly Core = typeof(Pair).Assembly;

    [TestCase("Expressif.Values.PairValue")]
    [TestCase("Expressif.Values.TupleValue")]
    [TestCase("Expressif.Values.VectorValue")]
    [TestCase("Expressif.Values.DictionaryValue")]
    [TestCase("Expressif.Values.SortTermValue")]
    [TestCase("Expressif.Values.SortKeyValue")]
    [TestCase("Expressif.Values.ILiteDataRow")]
    [TestCase("Expressif.Values.Special.BaseSpecial")]
    public void ParallelPublicValueType_IsRemoved(string name)
        => Assert.That(Core.GetType(name), Is.Null);

    [TestCase(typeof(Pair))]
    [TestCase(typeof(TupleValue))]
    [TestCase(typeof(VectorValue))]
    [TestCase(typeof(DictionaryValue))]
    [TestCase(typeof(Group))]
    [TestCase(typeof(Grouping))]
    [TestCase(typeof(RecordValue))]
    [TestCase(typeof(SortTermValue))]
    [TestCase(typeof(SortKeyValue))]
    [TestCase(typeof(SortTableValue))]
    [TestCase(typeof(ContextVariables))]
    [TestCase(typeof(ContextObject))]
    [TestCase(typeof(Any))]
    [TestCase(typeof(Value))]
    [TestCase(typeof(Null))]
    [TestCase(typeof(Empty))]
    [TestCase(typeof(Whitespace))]
    public void CanonicalValueType_IsSealed(Type type)
        => Assert.That(type.IsSealed, Is.True);

    [TestCase(typeof(Any))]
    [TestCase(typeof(Value))]
    [TestCase(typeof(Null))]
    [TestCase(typeof(Empty))]
    [TestCase(typeof(Whitespace))]
    public void SpecialValue_ExposesOnlySingletonConstruction(Type type)
    {
        Assert.Multiple(() =>
        {
            Assert.That(type.GetConstructors(), Is.Empty);
            Assert.That(type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        });
    }

    [TestCase("Expressif.Values.Types.ITypeRegistry")]
    [TestCase("Expressif.Values.Types.TypeRegistry")]
    [TestCase("Expressif.Values.Types.TypeIntrospector")]
    [TestCase("Expressif.Values.Types.UnknownExpressifTypeException")]
    [TestCase("Expressif.Values.JsonValueParser")]
    [TestCase("Expressif.Values.NamedValueAccessor")]
    [TestCase("Expressif.Values.RecordSyntax")]
    [TestCase("Expressif.Values.Casters.TypeChecker")]
    [TestCase("Expressif.Values.Converters.DateOnlyConverter")]
    public void RuntimeInfrastructure_IsInternal(string name)
        => Assert.That(Core.GetType(name)!.IsPublic, Is.False);

    [Test]
    public void TypeAuthoringContracts_RemainPublic()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(ExpressifTypeAttribute).IsPublic, Is.True);
            Assert.That(typeof(ITypeDescriptor).IsPublic, Is.True);
            Assert.That(typeof(ExpressifTypeDefinition<>).IsPublic, Is.True);
            Assert.That(typeof(TypeDescriptor).GetConstructors(), Is.Empty);
            Assert.That(typeof(TypeLiteralMetadata).GetConstructors(), Is.Empty);
        });
    }
}
