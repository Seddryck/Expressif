using System;
using System.Reflection;

namespace Expressif
{
    abstract class ExpressifException : Exception
    {
        public ExpressifException(string message)
             : base(message)
        { }
    }

    class NotImplementedFunctionException : ExpressifException
    {
        public NotImplementedFunctionException(string className)
            : base($"The function named '{className}' is not implemented in this version of {Assembly.GetCallingAssembly().GetName().Name}.")
        { }
    }

    class MissingOrUnexpectedParametersFunctionException : ExpressifException
    {
        public MissingOrUnexpectedParametersFunctionException(string className, int parameterCount)
            : base($"The function named '{className}' is not expecting to receive {parameterCount} parameters.")
        { }
    }

    class InvalidIOException : ExpressifException
    {
        public InvalidIOException(string initialValue)
            : base($"Can't evaluate a file's property when the path of this file is equal to {initialValue}.")
        { }
    }
}
