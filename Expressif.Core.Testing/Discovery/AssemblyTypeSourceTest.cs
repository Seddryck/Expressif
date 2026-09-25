using System.Reflection;
using Expressif.Discovery;

namespace Expressif.Testing.Discovery;

public class AssemblyTypeSourceTest
{
    [Test]
    public void Type_IsSealedAndDoesNotExposeAssemblies()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(AssemblyTypeSource).IsSealed, Is.True);
            Assert.That(
                typeof(AssemblyTypeSource).GetProperties(BindingFlags.Instance | BindingFlags.Public),
                Has.None.Matches<PropertyInfo>(property => property.PropertyType == typeof(Assembly[])));
        });
    }

    [Test]
    public void GetTypes_ReturnsAllTypesWithoutFiltering()
    {
        var available = new[] { typeof(ITypeSource), typeof(Stream), typeof(string) };
        var assembly = AssemblyReturning(available);

        Assert.That(
            new AssemblyTypeSource(assembly.Object).GetTypes(),
            Is.EqualTo(available.OrderBy(type => type.AssemblyQualifiedName, StringComparer.Ordinal)));
    }

    [Test]
    public void GetTypes_PreservesAssemblyOrderAndDeduplicatesAssembliesAndTypes()
    {
        var firstTypes = new[] { typeof(string), typeof(int), typeof(string) };
        var secondTypes = new[] { typeof(decimal), typeof(int) };
        var first = AssemblyReturning(firstTypes);
        var second = AssemblyReturning(secondTypes);

        var actual = new AssemblyTypeSource(first.Object, first.Object, second.Object).GetTypes();
        var expected = firstTypes
            .OrderBy(type => type.AssemblyQualifiedName, StringComparer.Ordinal)
            .Concat(secondTypes.OrderBy(type => type.AssemblyQualifiedName, StringComparer.Ordinal))
            .Distinct();

        Assert.That(actual, Is.EqualTo(expected));
        first.Verify(assembly => assembly.GetTypes(), Times.Once);
        second.Verify(assembly => assembly.GetTypes(), Times.Once);
    }

    [Test]
    public void GetTypes_CachesSnapshotForRepeatedAndConcurrentEnumeration()
    {
        var assembly = AssemblyReturning([typeof(string), typeof(int)]);
        var source = new AssemblyTypeSource(assembly.Object);

        var first = source.GetTypes();
        Parallel.For(0, 50, _ => Assert.That(source.GetTypes().ToArray(), Is.EqualTo(first)));

        Assert.That(source.GetTypes(), Is.SameAs(first));
        assembly.Verify(candidate => candidate.GetTypes(), Times.Once);
    }

    [Test]
    public void GetTypes_ReflectionTypeLoadFailure_ReturnsLoadableTypes()
    {
        var failure = new ReflectionTypeLoadException(
            [typeof(string), null],
            [new TypeLoadException("Unavailable type")]);
        var assembly = new Mock<Assembly>();
        assembly.Setup(candidate => candidate.GetTypes()).Throws(failure);

        Assert.That(new AssemblyTypeSource(assembly.Object).GetTypes(), Is.EqualTo(new[] { typeof(string) }));
    }

    [Test]
    public void GetTypes_OtherFailure_IsPropagatedAndCached()
    {
        var expected = new BadImageFormatException("Invalid assembly");
        var assembly = new Mock<Assembly>();
        assembly.Setup(candidate => candidate.GetTypes()).Throws(expected);
        var source = new AssemblyTypeSource(assembly.Object);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Catch(() => source.GetTypes()), Is.SameAs(expected));
            Assert.That(Assert.Catch(() => source.GetTypes()), Is.SameAs(expected));
        });
        assembly.Verify(candidate => candidate.GetTypes(), Times.Once);
    }

    [Test]
    public void Constructor_NullSequence_Throws()
        => Assert.That(
            () => new AssemblyTypeSource((Assembly[])null!),
            Throws.ArgumentNullException);

    [Test]
    public void Constructor_NullAssembly_Throws()
        => Assert.That(
            () => new AssemblyTypeSource([null!]),
            Throws.ArgumentException);

    [Test]
    public void GetTypes_NoAssemblies_ReturnsEmptySnapshot()
        => Assert.That(new AssemblyTypeSource([]).GetTypes(), Is.Empty);

    private static Mock<Assembly> AssemblyReturning(Type[] types)
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(candidate => candidate.GetTypes()).Returns(types);
        return assembly;
    }
}
