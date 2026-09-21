using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Values.Casters;

public sealed class CasterValueConverter : IValueConverter
{
    private readonly Caster caster = new();

    public CasterValueConverter(ITypeSource source)
    { }

    public object? Convert(object? value, Type targetType)
    {
        var method = typeof(Caster).GetMethods()
            .Single(candidate => candidate.Name == nameof(Caster.Cast)
                && candidate.IsGenericMethodDefinition);
        try
        {
            return method.MakeGenericMethod(targetType).Invoke(caster, [value]);
        }
        catch (System.Reflection.TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }
}
