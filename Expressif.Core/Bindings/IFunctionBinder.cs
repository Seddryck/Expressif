using Expressif.Functions;
using Expressif.Syntax;
using Expressif.Values.Types;

namespace Expressif.Bindings;

/// <summary>
/// Binds the syntax of a function that requires specialized argument handling.
/// </summary>
public interface IFunctionBinder
{
    Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context);
}

/// <summary>
/// Associates a specialized syntax binder with a runtime function implementation.
/// </summary>
/// <typeparam name="TFunction">The runtime function implementation.</typeparam>
public interface IFunctionBinder<TFunction> : IFunctionBinder
    where TFunction : IFunction
{ }

/// <summary>
/// Provides recursive expression binding to specialized function binders.
/// </summary>
public interface IFunctionBindingContext
{
    IParameter BindArgument(ExpressionSyntax syntax);
    TypeDescriptor ResolveType(string name);
    Type? ResolveRuntimeType(string name);
}
