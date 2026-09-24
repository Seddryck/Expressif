using System.Reflection;

namespace Expressif.Bindings;

internal sealed record ParameterArgumentBinding(ConstructorInfo Constructor, IParameter[] Parameters, bool[] Supplied)
{
    public ParameterArgumentBinding(ConstructorInfo constructor, IParameter[] parameters)
        : this(constructor, parameters, Enumerable.Repeat(true, parameters.Length).ToArray()) { }
}

internal static class ParameterArgumentBinder
{
    public static ParameterArgumentBinding Bind(Type type, FunctionArgument[] arguments)
    {
        return Bind(type, arguments, type.GetConstructors());
    }

    internal static ParameterArgumentBinding Bind(Type type, FunctionArgument[] arguments, ConstructorInfo[] constructors)
    {
        var positionalCount = arguments.TakeWhile(x => x.Name is null).Count();
        var named = arguments.Skip(positionalCount).ToArray();
        var functionName = type.Name.ToKebabCase();

        if (positionalCount > constructors.Max(x => x.GetParameters().Length))
            throw new TooManyPositionalArgumentsException(functionName);

        var allParameters = constructors.SelectMany(x => x.GetParameters()).ToArray();
        var suppliedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var argument in named)
        {
            if (!suppliedNames.Add(argument.Name!.ToKebabCase()))
                throw new DuplicateNamedArgumentException(argument.Name!);

            if (!allParameters.Any(x => NamesMatch(x.Name!, argument.Name!)))
                throw new UnknownParameterNameException(functionName, argument.Name!);

            if (constructors.Any(x => x.GetParameters().Take(positionalCount)
                .Any(p => NamesMatch(p.Name!, argument.Name!))))
                throw new PositionallySuppliedParameterException(argument.Name!);
        }

        var matches = constructors.Select(x => TryBind(x, arguments, positionalCount))
            .Where(x => x is not null).Cast<ParameterArgumentBinding>().ToArray();
        if (matches.Length == 1)
            return matches[0];
        if (matches.Length > 1)
        {
            var exact = matches.Where(match => match.Constructor.GetParameters().Length == arguments.Length).ToArray();
            if (exact.Length == 1)
                return exact[0];
            throw new AmbiguousParameterBindingException(functionName);
        }

        var candidate = constructors.Where(x => x.GetParameters().Length >= positionalCount)
            .OrderByDescending(x => x.GetParameters().Length).First();
        var missing = candidate.GetParameters().Skip(positionalCount)
            .FirstOrDefault(x => !x.IsOptional && !suppliedNames.Contains(x.Name!.ToKebabCase()));
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
        var supplied = new bool[metadata.Length];
        for (var i = 0; i < positionalCount; i++)
        {
            values[i] = arguments[i].Value;
            supplied[i] = true;
        }
        foreach (var argument in arguments.Skip(positionalCount))
        {
            var index = Array.FindIndex(metadata, x => NamesMatch(x.Name!, argument.Name!));
            if (index < 0)
                return null;
            values[index] = argument.Value;
            supplied[index] = true;
        }
        if (values.Select((value, index) => value is null && !metadata[index].IsOptional).Any(x => x))
            return null;
        for (var i = 0; i < values.Length; i++)
            values[i] ??= new LiteralParameter(metadata[i].DefaultValue);
        return new ParameterArgumentBinding(constructor, values!, supplied);
    }

    private static bool NamesMatch(string parameter, string supplied)
        => parameter.ToKebabCase().Equals(supplied.ToKebabCase(), StringComparison.OrdinalIgnoreCase);
}
