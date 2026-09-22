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

internal static class ProbeService
{
    public static T Create<T>(ITypesProbe probe)
    {
        var serviceType = probe.Locate().SingleOrDefault(type => typeof(T).IsAssignableFrom(type))
            ?? throw new InvalidOperationException(
                $"No implementation of '{typeof(T).FullName}' was found by the supplied type probe.");
        var probeConstructor = serviceType.GetConstructor([typeof(ITypesProbe)]);
        return (T)(probeConstructor is not null
            ? probeConstructor.Invoke([probe])
            : Activator.CreateInstance(serviceType, nonPublic: true)
                ?? throw new InvalidOperationException(
                    $"Service '{serviceType.FullName}' must have a parameterless or ITypesProbe constructor."));
    }
}
