using System.Collections;
using System.CommandLine;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using PocketCsvReader;
using Expressif.Values;
using Expressif.Parsers;
using Expressif.Serializers;
using Sprache;

namespace Expressif.Cli.Commands;

internal static class RunCommand
{
    private static readonly Func<string, Context, Expression> DefaultBuildExpression = static (code, context) => new Expression(code, context);
    private static readonly Func<string, Context, ClosedExpression> DefaultBuildClosedExpression = static (code, context) => new ClosedExpression(code, context);
    private static readonly Func<ClosedExpression, object?> DefaultEvaluateClosedExpression = static expression => expression.Evaluate();
    private static readonly Func<string, object?> DefaultParseInput = static input => InputValueParser.Parse(input);
    private static readonly Func<string, object?> DefaultResolveSourceValue = ResolveSourceValueCore;
    private static readonly Func<Expression, Context, IEnumerable, IEnumerable<object?>> DefaultRunExpression = static (expression, context, inputs) => EvaluateEach(expression, context, inputs);

    internal static Func<string, Context, Expression> BuildExpression { get; set; }
        = DefaultBuildExpression;

    internal static Func<string, Context, ClosedExpression> BuildClosedExpression { get; set; }
        = DefaultBuildClosedExpression;

    internal static Func<ClosedExpression, object?> EvaluateClosedExpression { get; set; }
        = DefaultEvaluateClosedExpression;

    internal static Func<string, object?> ParseInput { get; set; }
        = DefaultParseInput;

    internal static Func<string, object?> ResolveSourceValue { get; set; }
        = DefaultResolveSourceValue;

    internal static Func<Expression, Context, IEnumerable, IEnumerable<object?>> RunExpression { get; set; }
        = DefaultRunExpression;

    internal static void ResetDelegates()
    {
        BuildExpression = DefaultBuildExpression;
        BuildClosedExpression = DefaultBuildClosedExpression;
        EvaluateClosedExpression = DefaultEvaluateClosedExpression;
        ParseInput = DefaultParseInput;
        ResolveSourceValue = DefaultResolveSourceValue;
        RunExpression = DefaultRunExpression;
    }

    public static Command Create()
    {
        var expressionArgument = new Argument<string?>("expression")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Expression to evaluate."
        };

        var inputOption = new Option<string[]>("--input")
        {
            Description = "Input row passed to the expression. Repeat --input to add rows."
        };
        inputOption.Aliases.Add("-i");

        var batchOption = new Option<string?>("--batch")
        {
            Description = "Enumerable batch input. Each direct element is evaluated as one row."
        };

        var expressionFileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 file containing the expression to evaluate."
        };
        expressionFileOption.Aliases.Add("-f");

        var sourceOption = new Option<string?>("--source")
        {
            Description = "Path to a source file returning rows as IEnumerable or IDataReader."
        };
        sourceOption.Aliases.Add("-s");

        var scalarOption = new Option<bool>("--scalar")
        {
            Description = "Treat each source row as a single value. The source must contain exactly one column."
        };

        var command = new Command("run", "Evaluate an Expressif expression for each element of an input sequence.");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(inputOption);
        command.Options.Add(batchOption);
        command.Options.Add(sourceOption);
        command.Options.Add(scalarOption);
        command.Options.Add(expressionFileOption);

        command.SetAction(parseResult =>
        {
            var inlineExpression = parseResult.GetValue(expressionArgument);
            var expressionFilePath = parseResult.GetValue(expressionFileOption);
            var sourcePath = parseResult.GetValue(sourceOption);
            var scalar = parseResult.GetValue(scalarOption);
            var inputRows = parseResult.GetValue(inputOption) ?? [];
            var batchInput = parseResult.GetValue(batchOption);
            var hasInputOption = parseResult.GetResult(inputOption) is not null;
            var hasBatchOption = parseResult.GetResult(batchOption) is not null;
            var hasSourceOption = parseResult.GetResult(sourceOption) is not null;

            if (scalar && !hasSourceOption)
            {
                Console.Error.WriteLine("The --scalar option requires --source.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            var batchOptionOccurrences = parseResult.Tokens.Count(token => token.Value is "--batch");
            if (batchOptionOccurrences > 1)
            {
                Console.Error.WriteLine("The --batch option can only be specified once.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!ExpressionCommandCommon.TryResolveExpressionCode(
                    inlineExpression,
                    expressionFilePath,
                    out var expressionCode,
                    out var hasExpressionFile))
            {
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!hasInputOption && !hasBatchOption && !hasSourceOption)
            {
                Console.Error.WriteLine("The run command requires inputs. Provide --input, --batch, or --source.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (hasSourceOption && (hasInputOption || hasBatchOption))
            {
                Console.Error.WriteLine("The --source option cannot be combined with --input or --batch.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            var sequenceInput = hasSourceOption
                ? BuildSourceRows(sourcePath, scalar)
                : BuildInputSequence(inputRows, hasBatchOption, batchInput);

            var context = new Context();
            Expression openExpression;
            try
            {
                openExpression = BuildExpression(expressionCode, context);
            }
            catch (Exception exception) when (exception is Sprache.ParseException
                                              or NotImplementedFunctionException
                                              or MissingOrUnexpectedParametersFunctionException)
            {
                return ExpressionCommandCommon.WriteValidationError(exception, hasExpressionFile, expressionFilePath);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unexpected error: {exception.Message}");
                return ExitCodes.UnexpectedInternalError;
            }

            try
            {
                foreach (var result in RunExpression(openExpression, context, sequenceInput))
                    Console.Out.WriteLine(ValueFormatter.Format(result));

                return ExitCodes.Success;
            }
            catch (FormatException exception)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.InvalidExpressionOrInput;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.EvaluationFailed;
            }
        });

        return command;
    }

    private static IEnumerable<object?> EvaluateEach(Expression expression, Context context, IEnumerable inputs)
    {
        var enumerator = inputs.GetEnumerator();
        var index = 0;
        try
        {
            while (true)
            {
                object? input;
                try
                {
                    if (!enumerator.MoveNext())
                        yield break;

                    input = enumerator.Current;
                }
                catch (Exception exception) when (exception is not OutOfMemoryException and not FormatException)
                {
                    throw new InvalidOperationException(
                        $"Input enumeration failed at position {index}: {exception.Message}",
                        exception);
                }

                object? result;
                try
                {
                    context.CurrentObject.Set(input);
                    result = expression.Evaluate(input);
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    var rootCause = exception is TargetInvocationException invocationException
                        ? invocationException.InnerException ?? exception
                        : exception;

                    throw new InvalidOperationException(
                        $"Expression evaluation failed for input {FormatValue(input)}: {rootCause.Message}",
                        rootCause);
                }

                yield return result;
                index++;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private static IEnumerable<object?> BuildInputSequence(
        IEnumerable<string> inputRows,
        bool hasBatchOption,
        string? batchInput)
    {
        foreach (var input in inputRows)
        {
            object? parsedInput;
            try
            {
                parsedInput = ParseInput(input);
            }
            catch (FormatException exception)
            {
                throw new FormatException($"Invalid input syntax for --input '{input}': {exception.Message}", exception);
            }

            // Each --input occurrence defines one row as-is, including array values.
            yield return parsedInput;
        }

        if (!hasBatchOption)
            yield break;

        object? parsedBatchInput;
        try
        {
            parsedBatchInput = ParseInput(batchInput ?? string.Empty);
        }
        catch (FormatException exception)
        {
            throw new FormatException($"Invalid input syntax for --batch '{batchInput}': {exception.Message}", exception);
        }

        if (parsedBatchInput is not IEnumerable enumerable || parsedBatchInput is string or RecordValue)
            throw new FormatException("The --batch option requires an enumerable value.");

        var enumerator = enumerable.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    internal static IEnumerable<object?> BuildSourceRows(string? sourcePath, bool scalar = false)
    {
        object? sourceValue;
        try
        {
            sourceValue = ResolveSourceValue(sourcePath ?? string.Empty);
        }
        catch (FormatException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            throw new FormatException($"The source '{sourcePath}' could not be resolved: {exception.Message}", exception);
        }

        if (sourceValue is null)
            throw new FormatException("The source supplied to 'run' returned null. Expected an IEnumerable or IDataReader.");

        if (sourceValue is IDataReader reader)
        {
            foreach (var row in EnumerateReaderRows(reader, sourcePath ?? string.Empty, scalar))
                yield return row;

            yield break;
        }

        if (sourceValue is IEnumerable enumerable and not string)
        {
            if (scalar)
                throw new FormatException("The --scalar option requires a tabular source exposing exactly one column.");

            var enumerator = enumerable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }

            yield break;
        }

        throw new FormatException("The source supplied to 'run' returned a scalar value. Expected an IEnumerable or IDataReader.");
    }

    private static IEnumerable<object?> EnumerateReaderRows(IDataReader reader, string sourcePath, bool scalar)
    {
        var hasHeaderRecord = reader is DisposableDataReader wrappedReader && wrappedReader.HasHeaderRecord;

        if (hasHeaderRecord)
        {
            foreach (var row in EnumerateCsvRows(reader, sourcePath, scalar))
                yield return row;

            yield break;
        }

        foreach (var row in EnumerateGenericReaderRows(reader, sourcePath, scalar))
            yield return row;
    }

    private static IEnumerable<object?> EnumerateGenericReaderRows(IDataReader reader, string sourcePath, bool scalar)
    {
        try
        {
            ValidateScalarColumnCount(reader.FieldCount, sourcePath, scalar);
            while (true)
            {
                bool hasRow;
                try
                {
                    hasRow = reader.Read();
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    throw new FormatException($"Reading from source '{sourcePath}' failed: {exception.Message}", exception);
                }

                if (!hasRow)
                    yield break;

                yield return scalar ? GetValue(reader, 0) : BuildLiteDataRow(reader);
            }
        }
        finally
        {
            reader.Dispose();
        }
    }

    private static IEnumerable<object?> EnumerateCsvRows(IDataReader reader, string sourcePath, bool scalar)
    {
        try
        {
            bool hasHeader;
            try
            {
                hasHeader = reader.Read();
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                throw new FormatException($"Invalid CSV syntax in '{sourcePath}': {exception.Message}", exception);
            }

            if (!hasHeader)
                throw new FormatException($"CSV source '{sourcePath}' is empty. A header row is required.");

            var expectedFields = reader.FieldCount;
            if (expectedFields == 0)
                throw new FormatException($"CSV source '{sourcePath}' is empty. A header row is required.");

            ValidateScalarColumnCount(expectedFields, sourcePath, scalar);

            var headers = new string[expectedFields];
            var headerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < expectedFields; i++)
            {
                var header = Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture) ?? string.Empty;
                if (string.IsNullOrEmpty(header))
                    throw new FormatException($"CSV header in '{sourcePath}' is invalid: field {i + 1} is empty.");

                if (!headerSet.Add(header))
                    throw new FormatException($"CSV header in '{sourcePath}' contains duplicate column name '{header}'.");

                headers[i] = header;
            }

            var recordNumber = 2;
            while (true)
            {
                bool hasRow;
                try
                {
                    hasRow = reader.Read();
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    throw new FormatException($"Invalid CSV syntax in '{sourcePath}': {exception.Message}", exception);
                }

                if (!hasRow)
                    yield break;

                var actualFields = reader.FieldCount;
                if (actualFields != expectedFields)
                    throw new FormatException($"CSV record {recordNumber} in '{sourcePath}' contains {actualFields} fields, but {expectedFields} fields were expected.");

                var values = new object?[expectedFields];
                for (var i = 0; i < expectedFields; i++)
                {
                    var value = reader.GetValue(i);
                    values[i] = value is DBNull ? null : value;
                }

                yield return scalar ? values[0] : new LiteDataRow(headers, values);
                recordNumber++;
            }
        }
        finally
        {
            reader.Dispose();
        }
    }

    private static void ValidateScalarColumnCount(int fieldCount, string sourcePath, bool scalar)
    {
        if (scalar && fieldCount != 1)
            throw new FormatException($"The --scalar option requires source '{sourcePath}' to contain exactly one column; found {fieldCount}.");
    }

    private static object? GetValue(IDataRecord record, int index)
    {
        var value = record.GetValue(index);
        return value is DBNull ? null : value;
    }

    private static LiteDataRow BuildLiteDataRow(IDataReader reader)
    {
        var fields = reader.FieldCount;
        var names = new string[fields];
        var values = new object?[fields];
        for (var i = 0; i < fields; i++)
        {
            names[i] = reader.GetName(i);
            var value = reader.GetValue(i);
            values[i] = value is DBNull ? null : value;
        }

        return new LiteDataRow(names, values);
    }

    private static object? ResolveSourceValueCore(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new FormatException("Source path is required.");

        if (Directory.Exists(sourcePath))
            throw new FormatException($"Source '{sourcePath}' is a directory.");

        if (!File.Exists(sourcePath))
            throw new FormatException($"Source '{sourcePath}' was not found.");

        if (Path.GetExtension(sourcePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return OpenCsvDataReader(sourcePath);

        var sourceCode = ReadUtf8File(sourcePath);
        ClosedExpression closedExpression;
        try
        {
            closedExpression = BuildClosedExpression(sourceCode, new Context());
        }
        catch (Exception exception) when (exception is Sprache.ParseException
                                          or NotImplementedFunctionException
                                          or MissingOrUnexpectedParametersFunctionException
                                          or ExpressionRequiresInputException)
        {
            throw new FormatException($"The source '{sourcePath}' is invalid: {CommandErrorFormatter.FormatValidationError(exception)}", exception);
        }

        try
        {
            return EvaluateClosedExpression(closedExpression);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            throw new FormatException($"The source '{sourcePath}' could not be evaluated: {exception.Message}", exception);
        }
    }

    private static IDataReader OpenCsvDataReader(string sourcePath)
    {
        FileStream? stream = null;
        try
        {
            stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var csvReader = new CsvReader(CsvProfile.CommaDoubleQuote);
            var csvDataReader = csvReader.ToDataReader(stream);
            return new DisposableDataReader(csvDataReader, stream, hasHeaderRecord: true);
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }

    private static string ReadUtf8File(string sourcePath)
    {
        try
        {
            using var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(
                stream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }
        catch (DecoderFallbackException exception)
        {
            throw new FormatException($"Source '{sourcePath}' could not be decoded as UTF-8.", exception);
        }
        catch (Exception exception) when (exception is IOException
                                          or UnauthorizedAccessException
                                          or ArgumentException
                                          or NotSupportedException)
        {
            throw new FormatException($"Source '{sourcePath}' could not be accessed: {exception.Message}", exception);
        }
    }

    private static string FormatValue(object? value)
        => ValueFormatter.Format(value);

    internal static class InputValueParser
    {
        private static readonly ParameterSerializer ParameterSerializer = new();

        public static object? Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            try
            {
                var parameter = Parameter.Parser.End().Parse(text);
                return ConvertToRuntimeValue(parameter);
            }
            catch (ParseException exception)
            {
                if (TryParseAutoQuotedScalar(text, out var autoQuotedValue))
                    return autoQuotedValue;

                throw new FormatException(exception.Message, exception);
            }
        }

        private static bool TryParseAutoQuotedScalar(string text, out object? value)
        {
            value = null;

            if (!ShouldAutoQuoteScalar(text))
                return false;

            var normalized = text.Trim();
            var candidate = BuildQuotedScalar(normalized);

            try
            {
                var parameter = Parameter.Parser.End().Parse(candidate);
                value = ConvertToRuntimeValue(parameter);
                return true;
            }
            catch (ParseException)
            {
                return false;
            }
        }

        private static bool ShouldAutoQuoteScalar(string text)
        {
            var normalized = text.Trim();
            if (normalized.Length == 0)
                return false;

            // Only quote plain scalars with internal whitespace; avoid changing typed/structured inputs.
            if (!HasInternalWhitespace(normalized))
                return false;

            return normalized.IndexOfAny(['{', '}', '[', ']', '@', '#', '|', ',', ':']) < 0;
        }

        private static bool HasInternalWhitespace(string text)
        {
            for (var i = 1; i < text.Length - 1; i++)
                if (char.IsWhiteSpace(text[i]))
                    return true;

            return false;
        }

        private static string BuildQuotedScalar(string text)
            => text.Contains('`')
                ? $"\"{RecordSyntax.EscapeDoubleQuoted(text)}\""
                : $"`{text}`";

        private static object? ConvertToRuntimeValue(IParameter parameter)
        {
            return parameter switch
            {
                LiteralParameter literal => RecordSyntax.TryParseTypedToken(literal.Value, out var typed)
                    ? typed
                    : literal.Value,
                QuotedLiteralParameter quoted => quoted.Value,
                ArrayParameter array => array.Values.Select(ConvertToRuntimeValue).ToArray(),
                RecordLiteralParameter record => ConvertRecord(record),
                _ => ParameterSerializer.Serialize(parameter),
            };
        }

        private static RecordValue ConvertRecord(RecordLiteralParameter record)
        {
            var value = new RecordValue();
            foreach (var field in record.Fields)
            {
                if (value.ContainsKey(field.Name))
                    throw new ArgumentException($"Duplicate field '{field.Name}' in record literal.");

                value.Set(field.Name, ConvertToRuntimeValue(field.Value));
            }

            return value;
        }
    }

    private sealed class LiteDataRow : Expressif.Values.ILiteDataRow
    {
        private readonly string[] names;
        private readonly object?[] values;
        private readonly Dictionary<string, int> ordinals;

        public LiteDataRow(string[] names, object?[] values)
        {
            this.names = names;
            this.values = values;
            ordinals = new Dictionary<string, int>(names.Length, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < names.Length; i++)
                if (!ordinals.ContainsKey(names[i]))
                    ordinals[names[i]] = i;
        }

        public int ColumnCount => values.Length;

        public IReadOnlyList<string> ColumnNames => names;

        public bool ContainsColumn(string columnName)
            => ordinals.ContainsKey(columnName);

        public object? this[string columnName]
            => ordinals.TryGetValue(columnName, out var index)
                ? values[index]
                : throw new ArgumentOutOfRangeException(nameof(columnName), columnName, $"Column '{columnName}' does not exist.");

        public object? this[int index]
            => values[index];
    }

    private sealed class DisposableDataReader : IDataReader
    {
        private readonly IDataReader inner;
        private readonly IDisposable owner;

        public DisposableDataReader(IDataReader inner, IDisposable owner, bool hasHeaderRecord = false)
        {
            this.inner = inner;
            this.owner = owner;
            HasHeaderRecord = hasHeaderRecord;
        }

        public bool HasHeaderRecord { get; }

        public int Depth => inner.Depth;
        public bool IsClosed => inner.IsClosed;
        public int RecordsAffected => inner.RecordsAffected;
        public int FieldCount => inner.FieldCount;
        public object this[int i] => inner[i];
        public object this[string name] => inner[name]!;

        public void Close()
        {
            try
            {
                inner.Close();
            }
            finally
            {
                owner.Dispose();
            }
        }
        public DataTable GetSchemaTable() => inner.GetSchemaTable()!;
        public bool NextResult() => inner.NextResult();
        public bool Read() => inner.Read();
        public void Dispose()
        {
            try
            {
                inner.Dispose();
            }
            finally
            {
                owner.Dispose();
            }
        }

        public bool GetBoolean(int i) => inner.GetBoolean(i);
        public byte GetByte(int i) => inner.GetByte(i);
        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
            => inner.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        public char GetChar(int i) => inner.GetChar(i);
        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
            => inner.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        public IDataReader GetData(int i) => inner.GetData(i);
        public string GetDataTypeName(int i) => inner.GetDataTypeName(i);
        public DateTime GetDateTime(int i) => inner.GetDateTime(i);
        public decimal GetDecimal(int i) => inner.GetDecimal(i);
        public double GetDouble(int i) => inner.GetDouble(i);
        public Type GetFieldType(int i) => inner.GetFieldType(i);
        public float GetFloat(int i) => inner.GetFloat(i);
        public Guid GetGuid(int i) => inner.GetGuid(i);
        public short GetInt16(int i) => inner.GetInt16(i);
        public int GetInt32(int i) => inner.GetInt32(i);
        public long GetInt64(int i) => inner.GetInt64(i);
        public string GetName(int i) => inner.GetName(i);
        public int GetOrdinal(string name) => inner.GetOrdinal(name);
        public string GetString(int i) => inner.GetString(i);
        public object GetValue(int i) => inner.GetValue(i);
        public int GetValues(object[] values) => inner.GetValues(values);
        public bool IsDBNull(int i) => inner.IsDBNull(i);
    }
}
