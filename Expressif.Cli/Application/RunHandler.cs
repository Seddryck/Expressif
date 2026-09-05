using Expressif.Bindings;
using Expressif.Cli.Commands;
using Expressif.Cli.Expressions;
using Expressif.Cli.Inputs;
using Expressif.Cli.Infrastructure;
using Expressif.Syntax;
using Expressif.Values;

namespace Expressif.Cli.Application;

internal sealed record RunRequest(
    string? InlineExpression,
    string? ExpressionFilePath,
    string[] InputRows,
    string? BatchInput,
    string? SourcePath,
    SourceFormat? SourceFormat,
    string[] SourceOptions,
    bool Scalar,
    bool HasInput,
    bool HasBatch,
    bool HasSource,
    bool HasSourceOptions,
    int BatchOccurrences,
    ValueFormat OutputStyle = ValueFormat.Compact);

internal sealed class RunHandler(
    IExpressionService expressions,
    IInputValueParser values,
    IStrictUtf8TextReader textFiles,
    SourcePipeline sources)
{
    public int Execute(RunRequest request)
    {
        var requestError = RunRequestValidator.Validate(request);
        if (requestError is not null)
        {
            Console.Error.WriteLine(requestError);
            return ExitCodes.InvalidExpressionOrInput;
        }

        if (!ExpressionCommandCommon.TryResolveExpressionCode(
                request.InlineExpression,
                request.ExpressionFilePath,
                textFiles,
                out var expressionCode,
                out var hasExpressionFile))
        {
            return ExitCodes.InvalidExpressionOrInput;
        }

        var inputs = request.HasSource
            ? sources.Read(request.SourcePath, request.SourceOptions, request.Scalar, request.SourceFormat)
            : BuildInputSource(request).Read();
        var context = new Context();
        IExpression expression;
        try
        {
            expression = expressions.CompileOpen(expressionCode, context);
        }
        catch (Exception exception) when (exception is ExpressifSyntaxException
                                          or BindingException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException)
        {
            return ExpressionCommandCommon.WriteValidationError(
                exception, expressionCode, hasExpressionFile, request.ExpressionFilePath);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Unexpected error: {exception.Message}");
            return ExitCodes.UnexpectedInternalError;
        }

        try
        {
            foreach (var result in RunEvaluator.Evaluate(expression, context, inputs))
                Console.Out.WriteLine(ValueFormatter.Format(result, request.OutputStyle));
            return ExitCodes.Success;
        }
        catch (FormatException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitCodes.InvalidExpressionOrInput;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            CommandDiagnosticWriter.WriteLine(exception.Message);
            return ExitCodes.EvaluationFailed;
        }
    }

    internal IEnumerable<object?> BuildSourceRows(
        string? sourcePath,
        IReadOnlyList<string> sourceOptions,
        bool scalar = false,
        SourceFormat? format = null)
        => sources.Read(sourcePath, sourceOptions, scalar, format);

    private IRunInputSource BuildInputSource(RunRequest request)
    {
        var repeated = new RepeatedInputSource(request.InputRows, values);
        return request.HasBatch
            ? new CompositeInputSource(repeated, new BatchInputSource(request.BatchInput, values))
            : repeated;
    }
}
