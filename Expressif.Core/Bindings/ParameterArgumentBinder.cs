using System.Reflection;

namespace Expressif.Bindings;

internal sealed record ParameterArgumentBinding(ConstructorInfo Constructor, IParameter[] Parameters, bool[] Supplied)
{
    public ParameterArgumentBinding(ConstructorInfo constructor, IParameter[] parameters)
        : this(constructor, parameters, Enumerable.Repeat(true, parameters.Length).ToArray()) { }
}

internal static class ParameterArgumentBinder
{
    internal static void ValidateLayoutMetadata(Type type)
    {
        var layouts = type.GetConstructors()
            .Select(constructor => (Constructor: constructor,
                Attribute: constructor.GetCustomAttribute<ArgumentLayoutAttribute>()))
            .Where(item => item.Attribute is not null).ToArray();
        if (layouts.Length > 1)
            throw new InvalidOperationException($"Function '{type.FullName}' has ambiguous argument layouts.");
        foreach (var (_, attribute) in layouts)
        {
            if (!Enum.IsDefined(attribute!.Kind))
                throw InvalidLayout(type, "unknown layout kind");
            if (attribute.MinimumCardinality < 0 || attribute.MaximumCardinality < attribute.MinimumCardinality)
                throw InvalidLayout(type, "cardinality bounds are invalid");
            if (attribute.Kind == ArgumentLayoutKind.PositionalThenNamed)
            {
                if (attribute.PositionalPrefix < 1 || attribute.MinimumCardinality <= attribute.PositionalPrefix)
                    throw InvalidLayout(type, "mixed layout requires a positional prefix followed by named arguments");
            }
            else if (attribute.PositionalPrefix != 0)
            {
                throw InvalidLayout(type, "positional prefix is valid only for mixed layouts");
            }
            if (attribute.RequireUniqueNames && attribute.Kind == ArgumentLayoutKind.Positional)
                throw InvalidLayout(type, "name uniqueness is not valid for positional layouts");
        }
    }

    internal static ArgumentLayoutBinding BindLayout(Type type, FunctionArgument[] arguments)
    {
        ValidateLayoutMetadata(type);
        var layout = type.GetConstructors().Select(constructor => constructor.GetCustomAttribute<ArgumentLayoutAttribute>())
            .SingleOrDefault(attribute => attribute is not null)
            ?? throw InvalidLayout(type, "no argument layout is declared");
        var function = type.Name.ToKebabCase();
        if (arguments.Any(argument => argument.IsSpread))
        {
            if (layout.Kind == ArgumentLayoutKind.Positional)
                throw new MissingOrUnexpectedParametersFunctionException(function, arguments.Length);
            throw new SpreadArgumentException($"Spread arguments are not supported by {function}.");
        }
        if (arguments.Length < layout.MinimumCardinality || arguments.Length > layout.MaximumCardinality)
        {
            if (layout.Kind == ArgumentLayoutKind.Positional)
                throw new MissingOrUnexpectedParametersFunctionException(function, arguments.Length);
            throw new BindingException(
                $"Function '{function}' expects {layout.MinimumCardinality} to {layout.MaximumCardinality} arguments.");
        }

        var prefix = layout.Kind switch
        {
            ArgumentLayoutKind.Positional => arguments.Length,
            ArgumentLayoutKind.Named => 0,
            ArgumentLayoutKind.PositionalThenNamed => layout.PositionalPrefix,
            _ => throw InvalidLayout(type, "unknown layout kind"),
        };
        if (arguments.Take(prefix).Any(argument => argument.Name is not null)
            || arguments.Skip(prefix).Any(argument => argument.Name is null))
        {
            if (layout.Kind == ArgumentLayoutKind.Positional)
                throw new MissingOrUnexpectedParametersFunctionException(function, arguments.Length);
            var expectation = layout.Kind switch
            {
                ArgumentLayoutKind.Positional => "positional arguments",
                ArgumentLayoutKind.Named => "named arguments",
                ArgumentLayoutKind.PositionalThenNamed =>
                    $"{layout.PositionalPrefix} positional argument(s) followed by named arguments",
                _ => "a supported argument layout",
            };
            throw new BindingException($"Function '{function}' expects {expectation}.");
        }
        var positional = arguments.Take(prefix).ToArray();
        var named = arguments.Skip(prefix).ToArray();
        if (layout.RequireUniqueNames)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            var duplicate = named.FirstOrDefault(argument => !names.Add(argument.Name!));
            if (duplicate is not null)
                throw new BindingException($"Duplicate named argument '{duplicate.Name}' in {function}(...).");
        }
        return new ArgumentLayoutBinding(positional, named);
    }

    private static InvalidOperationException InvalidLayout(Type type, string reason)
        => new($"Invalid argument layout metadata on '{type.FullName}': {reason}.");

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
