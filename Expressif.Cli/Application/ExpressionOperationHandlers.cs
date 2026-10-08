using Expressif.Bindings;
using Expressif.Cli.Expressions;
using Expressif.Cli.Inputs;
using Expressif.Cli.Commands;
using Expressif.Planning;
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
            or LogicalPlanBindingException
            or NotImplementedFunctionException
            or MissingOrUnexpectedParametersFunctionException;

    public static ExpressionRequiresInputException? InputRequired(Exception exception)
        => exception as ExpressionRequiresInputException
            ?? exception.InnerException as ExpressionRequiresInputException;
}

internal enum EvaluateInputKind
{
    Closed,
    Value,
    Source,
}

internal sealed record EvaluateRequest(
    ExpressionCommandSource Source,
    EvaluateInputKind InputKind,
    string? Input,
    IReadOnlyList<string> SourcePaths,
    IReadOnlyList<string> SourceOptions,
    bool Scalar,
    bool Collect);

internal sealed class EvaluateHandler(
    IExpressionService expressions,
    IInputValueParser values,
    SourcePipeline sources)
{
    public ExpressionOperationResult Execute(EvaluateRequest request)
    {
        if (request.InputKind == EvaluateInputKind.Closed)
            return EvaluateClosed(request.Source);

        object? input;
        try
        {
            input = request.InputKind == EvaluateInputKind.Source
                ? ReadSource(request)
                : values.Parse(request.Input ?? string.Empty);
        }
        catch (FormatException exception)
        {
            var message = request.InputKind == EvaluateInputKind.Value
                ? $"Invalid input syntax for --input '{request.Input}': {exception.Message}"
                : exception.Message;
            return new ExpressionInputFailure(message);
        }

        return EvaluateOpen(request.Source, input);
    }

    private object?[] ReadSource(EvaluateRequest request)
        => request.Collect
            ? sources.CollectJsonDocuments(request.SourcePaths)
            : sources.Read(request.SourcePaths.SingleOrDefault(), request.SourceOptions, request.Scalar).ToArray();

    private ExpressionOperationResult EvaluateClosed(ExpressionCommandSource source)
    {
        IExpression expression;
        try
        {
            expression = CompileClosed(source, new Context());
        }
        catch (Exception exception) when (ExpressionFailureClassifier.InputRequired(exception) is not null)
        {
            var openResult = ValidateOpen(source);
            return openResult is ExpressionSuccessResult
                ? new ExpressionInputRequiredFailure(ExpressionFailureClassifier.InputRequired(exception)!)
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

    private ExpressionOperationResult EvaluateOpen(ExpressionCommandSource source, object? input)
    {
        IExpression expression;
        try
        {
            expression = CompileOpen(source, new Context());
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

    private ExpressionOperationResult ValidateOpen(ExpressionCommandSource source)
    {
        try
        {
            _ = CompileOpen(source, new Context());
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

    private IExpression CompileOpen(ExpressionCommandSource source, Context context)
        => source.Plan is not null
            ? expressions.CompileOpen(source.Plan, context)
            : expressions.CompileOpen(source.Code!, context);

    private IExpression CompileClosed(ExpressionCommandSource source, Context context)
        => source.Plan is not null
            ? expressions.CompileClosed(source.Plan, context)
            : expressions.CompileClosed(source.Code!, context);

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
