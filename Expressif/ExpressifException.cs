using System;
using System.Reflection;

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
}
