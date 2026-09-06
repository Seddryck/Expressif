using System.Data;
using System.Text.Json;
using Expressif.Bindings;
using Expressif.Cli.Commands;
using Expressif.Cli.Expressions;
using Expressif.Cli.Inputs;
using Expressif.Serialization;
using Expressif.Syntax;
using PocketCsvReader;

namespace Expressif.Cli.Infrastructure;

internal sealed class SourceInfrastructure(
    IExpressionService expressions,
    IInputValueParser values,
    IStrictUtf8TextReader textFiles)
{
    public IEnumerable<object?> Normalize(object? sourceValue, string sourcePath, bool scalar = false)
        => SourceRows.Read(sourceValue, sourcePath, scalar);

    internal object? OpenExpressionSource(string sourcePath, IReadOnlyList<string> sourceOptions)
    {
        if (sourceOptions.Count > 0)
            throw new FormatException($"Source options are not supported for source '{sourcePath}'.");
        var sourceCode = ReadUtf8File(sourcePath);
        IExpression closedExpression;
        try
        {
            closedExpression = expressions.CompileClosed(sourceCode, new Context());
        }
        catch (Exception exception) when (exception is ExpressifSyntaxException
                                          or BindingException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException
                                          or ExpressionRequiresInputException)
        {
            throw new FormatException($"The source '{sourcePath}' is invalid: {CommandErrorFormatter.FormatValidationError(exception)}", exception);
        }

        try
        {
            return expressions.Evaluate(closedExpression, null);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            throw new FormatException($"The source '{sourcePath}' could not be evaluated: {exception.Message}", exception);
        }
    }

    internal object?[] OpenJsonSource(string sourcePath, IReadOnlyList<string> sourceOptions)
    {
        if (sourceOptions.Count > 0)
            throw new FormatException($"Source options are not supported for source '{sourcePath}'.");

        var source = ReadUtf8File(sourcePath);
        try
        {
            return JsonValueReader.ReadRows(source);
        }
        catch (JsonException exception)
        {
            throw new FormatException($"Invalid JSON syntax in '{sourcePath}': {exception.Message}", exception);
        }
    }

    internal IDataReader OpenCsvDataReader(string sourcePath, IReadOnlyList<string> sourceOptions)
    {
        var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
            return CsvValueReader.OpenDataReader(stream, sourceOptions.Select(Parse).ToArray(), leaveOpen: false);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    internal (CsvProfile Profile, bool HeadersAreRows) BuildCsvProfile(IReadOnlyList<string> options)
        => new CsvSourceProfileBuilder().Build(options.Select(Parse).ToArray());

    private CsvSourceOption Parse(string text)
    {
        var separator = text.IndexOf('=');
        if (separator <= 0)
            throw new FormatException($"Invalid source option '{text}'. Expected <name>=<value>.");

        var name = text[..separator].Trim();
        var suppliedValue = text[(separator + 1)..];
        try
        {
            return new CsvSourceOption(name, values.ParseStrict(suppliedValue), suppliedValue);
        }
        catch (FormatException exception)
        {
            throw InvalidSourceOption(name, suppliedValue, exception.Message);
        }
    }

    private static FormatException InvalidSourceOption(string name, string value, string reason)
        => new($"Invalid CSV source option '{name}' with value '{value}': {reason}");

    private string ReadUtf8File(string sourcePath)
    {
        try
        {
            return textFiles.Read(sourcePath, requireContent: false);
        }
        catch (TextFileReadException exception) when (exception.Kind == TextFileFailureKind.InvalidUtf8)
        {
            throw new FormatException($"Source '{sourcePath}' could not be decoded as UTF-8.", exception);
        }
        catch (TextFileReadException exception)
        {
            throw new FormatException($"Source '{sourcePath}' could not be accessed: {exception.Message}", exception);
        }
    }
}
