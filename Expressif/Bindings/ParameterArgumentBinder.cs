using System.Reflection;

namespace Expressif.Bindings;

internal sealed record ParameterArgumentBinding(ConstructorInfo Constructor, IParameter[] Parameters);

internal static class ParameterArgumentBinder
{
    public static ParameterArgumentBinding Bind(Type type, FunctionArgument[] arguments)
    {
        var constructors = type.GetConstructors();
        var positionalCount = arguments.TakeWhile(x => x.Name is null).Count();
        var named = arguments.Skip(positionalCount).ToArray();
        var functionName = type.Name.ToKebabCase();

        if (positionalCount > constructors.Max(x => x.GetParameters().Length))
            throw new TooManyPositionalArgumentsException(functionName);

        var allParameters = constructors.SelectMany(x => x.GetParameters()).ToArray();
        foreach (var argument in named)
        {
            if (!allParameters.Any(x => x.Name!.Equals(argument.Name, StringComparison.OrdinalIgnoreCase)))
                throw new UnknownParameterNameException(functionName, argument.Name!);

            if (constructors.Any(x => x.GetParameters().Take(positionalCount)
                .Any(p => p.Name!.Equals(argument.Name, StringComparison.OrdinalIgnoreCase))))
                throw new PositionallySuppliedParameterException(argument.Name!);
        }

        var matches = constructors.Select(x => TryBind(x, arguments, positionalCount))
            .Where(x => x is not null).Cast<ParameterArgumentBinding>().ToArray();
        if (matches.Length == 1)
            return matches[0];
        if (matches.Length > 1)
            throw new AmbiguousParameterBindingException(functionName);

        var candidate = constructors.Where(x => x.GetParameters().Length >= positionalCount)
            .OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
        var suppliedNames = named.Select(x => x.Name!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = candidate?.GetParameters().Skip(positionalCount)
            .FirstOrDefault(x => !x.IsOptional && !suppliedNames.Contains(x.Name!));
        if (missing is not null)
            throw new MissingRequiredParameterException(missing.Name!);
        throw new AmbiguousParameterBindingException(functionName);
    }

    private static ParameterArgumentBinding? TryBind(ConstructorInfo constructor, FunctionArgument[] arguments, int positionalCount)
    {
        var metadata = constructor.GetParameters();
        if (positionalCount > metadata.Length)
            return null;

        var values = new IParameter?[metadata.Length];
        for (var i = 0; i < positionalCount; i++)
            values[i] = arguments[i].Value;
        foreach (var argument in arguments.Skip(positionalCount))
        {
            var index = Array.FindIndex(metadata, x => x.Name!.Equals(argument.Name, StringComparison.OrdinalIgnoreCase));
            if (index < positionalCount)
                return null;
            if (index < 0 || values[index] is not null)
                return null;
            values[index] = argument.Value;
        }
        if (values.Select((value, index) => value is null && !metadata[index].IsOptional).Any(x => x))
            return null;
        for (var i = 0; i < values.Length; i++)
            values[i] ??= new LiteralParameter(metadata[i].DefaultValue);
        return new ParameterArgumentBinding(constructor, values!);
    }
}
