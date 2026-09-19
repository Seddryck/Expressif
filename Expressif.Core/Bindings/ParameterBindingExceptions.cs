namespace Expressif.Bindings;

public abstract class ParameterBindingException(string message) : BindingException(message);

public sealed class PositionalArgumentAfterNamedArgumentException(string functionName)
    : ParameterBindingException($"Function '{functionName}' has a positional argument after a named argument.");

public sealed class UnknownParameterNameException(string functionName, string parameterName)
    : ParameterBindingException($"Function '{functionName}' has no parameter named '{parameterName}'.");

public sealed class DuplicateNamedArgumentException(string parameterName)
    : ParameterBindingException($"Parameter '{parameterName}' was specified more than once.");

public sealed class PositionallySuppliedParameterException(string parameterName)
    : ParameterBindingException($"Parameter '{parameterName}' was already supplied positionally.");

public sealed class MissingRequiredParameterException(string parameterName)
    : ParameterBindingException($"Required parameter '{parameterName}' was not supplied.");

public sealed class TooManyPositionalArgumentsException(string functionName)
    : ParameterBindingException($"Function '{functionName}' has too many positional arguments.");

public sealed class AmbiguousParameterBindingException(string functionName)
    : ParameterBindingException($"Function '{functionName}' has an ambiguous or unsupported overload after binding.");
