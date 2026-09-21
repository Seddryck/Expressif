using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Values;

namespace Expressif.Functions;

public interface IValueConverter
{
    object? Convert(object? value, Type targetType);
}

public interface IPredicationFactory
{
    Predicates.IPredicate Instantiate(IPredication predication, IContext context);
}

public interface ITupleFunctionInvoker
{
    Type ResolveTarget(
        string name,
        IImplementationRegistry functions,
        IImplementationRegistry predicates,
        Syntax.SourceSpan? sourceSpan = null);
    object? Invoke(
        string name,
        IPositionalValue tuple,
        IImplementationRegistry functions,
        IImplementationRegistry predicates,
        Syntax.SourceSpan? sourceSpan = null);
}

internal static class TypeSourceService
{
    public static T Create<T>(ITypeSource source)
    {
        var serviceType = source.GetTypes().SingleOrDefault(type => type.IsClass && !type.IsAbstract
            && typeof(T).IsAssignableFrom(type))
            ?? throw new InvalidOperationException(
                $"No implementation of '{typeof(T).FullName}' was found by the supplied type source.");
        var sourceConstructor = serviceType.GetConstructor([typeof(ITypeSource)]);
        return (T)(sourceConstructor is not null
            ? sourceConstructor.Invoke([source])
            : Activator.CreateInstance(serviceType, nonPublic: true)
                ?? throw new InvalidOperationException(
                    $"Service '{serviceType.FullName}' must have a parameterless or ITypeSource constructor."));
    }
}
