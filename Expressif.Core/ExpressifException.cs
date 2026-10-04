using System;
using System.Reflection;
using Expressif.Values;
using Expressif.Types;

namespace Expressif;

public abstract class ExpressifException : Exception
{
    public ExpressifException(string message)
         : base(message)
    { }
}

public class NotImplementedFunctionException : ExpressifException
{
    public NotImplementedFunctionException(string className)
        : base($"The function named '{className}' is not implemented in this version of {Assembly.GetCallingAssembly().GetName().Name}.")
    { }
}

public class MissingOrUnexpectedParametersFunctionException : ExpressifException
{
    public MissingOrUnexpectedParametersFunctionException(string className, int parameterCount)
        : base($"The function named '{className}' is not expecting to receive {parameterCount} parameters.")
    { }
}

public class InvalidIOException : ExpressifException
{
    public InvalidIOException(string initialValue)
        : base($"Can't evaluate a file's property when the path of this file is equal to {initialValue}.")
    { }
}

public class VariableAlreadyExistingException : ExpressifException
{
    public VariableAlreadyExistingException(string name)
        : base($"There is already a variable named '{name}' available in the context.")
    { }
}

public class UnexpectedVariableException : ExpressifException
{
    public UnexpectedVariableException(string name)
        : base($"There is no variable named '{name}' in the context.")
    { }
}

public class NotIndexableContextObjectException : ExpressifException
{
    public NotIndexableContextObjectException(object? value)
        : base($"The current object of the context of type '{value?.GetType().Name ?? "null"}' is not being accessible with the usage of a numeric index.")
    { }
}

public class NotNameableContextObjectException : ExpressifException
{
    public NotNameableContextObjectException(object? value)
        : base($"The current object of the context of type '{value?.GetType().Name ?? "null"}' is not being accessible with properties' name.")
    { }

    internal NotNameableContextObjectException(object? value, string name)
        : base(FormatMessage(value, name))
    { }

    private static string FormatMessage(object? value, string name)
    {
        var type = value is System.Collections.IList
            ? "array"
            : value is IExpressifValueType
                ? ExpressifTypeName.Get(value.GetType())
                : value?.GetType().Name ?? "null";

        return type switch
        {
            "array" => FormatArrayMessage(name),
            "tuple" => $"Cannot access field '{name}' directly on a tuple. Select a position such as $0 before accessing a field.",
            "pair" or "group" => $"Cannot access field '{name}' directly on a {type}. Select $key for its key or $value for its value before accessing a field.",
            _ => $"Cannot access field '{name}' directly on a value of type '{type}'. This type does not expose named fields.",
        };
    }

    private static string FormatArrayMessage(string name)
    {
        var field = FormatFieldExpression(name);
        return $"Cannot access field '{name}' directly on an array. Use map({field}) to access it on each element, or value-at(0) | {field} to access it on a specific element.";
    }

    private static string FormatFieldExpression(string name)
        => CanUseFieldShorthand(name)
            ? $".{name}"
            : $"field(\"{RecordSyntax.EscapeDoubleQuoted(name)}\")";

    private static bool CanUseFieldShorthand(string name)
        => name.Length > 0
            && IsAsciiLetter(name[0])
            && name.Skip(1).All(character => IsAsciiLetter(character)
                || char.IsAsciiDigit(character)
                || character is '_' or '-' or '+');

    private static bool IsAsciiLetter(char character)
        => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}

public class ExpressionRequiresInputException : ExpressifException
{
    public ExpressionRequiresInputException(string? reference)
        : base(reference is null
            ? "The expression is valid but requires an input to be evaluated."
            : $"The expression cannot be evaluated without an input because it references '{reference}'.")
    { }
}

public sealed class SpreadArgumentException(string message) : ExpressifException(message);

public sealed class StructuralValidationException(string message) : ExpressifException(message);
