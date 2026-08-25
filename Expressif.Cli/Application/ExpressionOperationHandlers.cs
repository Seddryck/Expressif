using Expressif.Bindings;
using Expressif.Cli.Expressions;
using Expressif.Cli.Inputs;
using Expressif.Syntax;

namespace Expressif.Cli.Application;

internal abstract record ExpressionOperationResult;

internal sealed record ExpressionSuccessResult(object? Value = null, bool HasValue = false) : ExpressionOperationResult;

internal sealed record ExpressionValidationFailure(Exception Exception) : ExpressionOperationResult;

internal sealed record ExpressionInputRequiredFailure(ExpressionRequiresInputException Exception) : ExpressionOperationResult;

internal sealed record ExpressionInputFailure(string Message) : ExpressionOperationResult;

internal sealed record ExpressionEvaluationFailure(Exception Exception) : ExpressionOperationResult;

internal sealed record ExpressionUnexpectedFailure(Exception Exception) : ExpressionOperationResult;

internal static class ExpressionFailureClassifier
{
    public static bool IsValidation(Exception exception)
        => exception is ExpressifSyntaxException
            or BindingException
            or NotImplementedFunctionException
            or MissingOrUnexpectedParametersFunctionException;
}

internal enum EvaluateInputKind
{
    Closed,
    Value,
    Source,
}

internal sealed record EvaluateRequest(
    string Expression,
    EvaluateInputKind InputKind,
    string? Input,
    string? SourcePath,
    IReadOnlyList<string> SourceOptions,
    bool Scalar);

internal sealed class EvaluateHandler(
    IExpressionService expressions,
    IInputValueParser values,
    SourcePipeline sources)
{
    public ExpressionOperationResult Execute(EvaluateRequest request)
    {
        if (request.InputKind == EvaluateInputKind.Closed)
            return EvaluateClosed(request.Expression);

        object? input;
        try
        {
            input = request.InputKind == EvaluateInputKind.Source
                ? sources.Read(request.SourcePath, request.SourceOptions, request.Scalar).ToArray()
                : values.Parse(request.Input ?? string.Empty);
        }
        catch (FormatException exception)
        {
            var message = request.InputKind == EvaluateInputKind.Value
                ? $"Invalid input syntax for --input '{request.Input}': {exception.Message}"
                : exception.Message;
            return new ExpressionInputFailure(message);
        }

        return EvaluateOpen(request.Expression, input);
    }

    private ExpressionOperationResult EvaluateClosed(string code)
    {
        IExpression expression;
        try
        {
            expression = expressions.CompileClosed(code, new Context());
        }
        catch (ExpressionRequiresInputException exception)
        {
            var openResult = ValidateOpen(code);
            return openResult is ExpressionSuccessResult
                ? new ExpressionInputRequiredFailure(exception)
                : openResult;
        }
        catch (Exception exception) when (ExpressionFailureClassifier.IsValidation(exception))
        {
            return new ExpressionValidationFailure(exception);
        }
        catch (Exception exception)
        {
            return new ExpressionUnexpectedFailure(exception);
        }

        return Evaluate(expression, null);
    }

    private ExpressionOperationResult EvaluateOpen(string code, object? input)
    {
        IExpression expression;
        try
        {
            expression = expressions.CompileOpen(code, new Context());
        }
        catch (Exception exception) when (ExpressionFailureClassifier.IsValidation(exception))
        {
            return new ExpressionValidationFailure(exception);
        }
        catch (Exception exception)
        {
            return new ExpressionUnexpectedFailure(exception);
        }

        return Evaluate(expression, input);
    }

    private ExpressionOperationResult ValidateOpen(string code)
    {
        try
        {
            _ = expressions.CompileOpen(code, new Context());
            return new ExpressionSuccessResult();
        }
        catch (Exception exception) when (ExpressionFailureClassifier.IsValidation(exception))
        {
            return new ExpressionValidationFailure(exception);
        }
        catch (Exception exception)
        {
            return new ExpressionUnexpectedFailure(exception);
        }
    }

    private ExpressionOperationResult Evaluate(IExpression expression, object? input)
    {
        try
        {
            return new ExpressionSuccessResult(expressions.Evaluate(expression, input), HasValue: true);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return new ExpressionEvaluationFailure(exception);
        }
    }
}

internal sealed record ValidateRequest(string Expression, bool Closed);

internal sealed class ValidateHandler(IExpressionService expressions)
{
    public ExpressionOperationResult Execute(ValidateRequest request)
    {
        try
        {
            _ = request.Closed
                ? expressions.CompileClosed(request.Expression, new Context())
                : expressions.CompileOpen(request.Expression, new Context());
            return new ExpressionSuccessResult();
        }
        catch (ExpressionRequiresInputException exception)
        {
            return new ExpressionInputRequiredFailure(exception);
        }
        catch (Exception exception) when (ExpressionFailureClassifier.IsValidation(exception))
        {
            return new ExpressionValidationFailure(exception);
        }
        catch (Exception exception)
        {
            return new ExpressionUnexpectedFailure(exception);
        }
    }
}
