using System.Collections;
using System.Data;
using System.Globalization;
using PocketCsvReader;
using Expressif.Values;
using Expressif.Bindings;
using Expressif.Syntax;

using Expressif.Cli.Commands;
using Expressif.Cli.Expressions;
using Expressif.Cli.Inputs;
namespace Expressif.Cli.Infrastructure;

internal sealed class SourceInfrastructure(
    IExpressionService expressions,
    IInputValueParser values,
    IStrictUtf8TextReader textFiles)
{
    public IEnumerable<object?> Normalize(object? sourceValue, string sourcePath, bool scalar = false)
    {
        foreach (var row in CreateSourceRows(sourceValue, sourcePath, scalar))
            yield return row;
    }

    private static IEnumerable<object?> CreateSourceRows(object? sourceValue, string sourcePath, bool scalar)
        => sourceValue switch
        {
            null => throw new FormatException("The source returned null. Expected an IEnumerable or IDataReader."),
            IDataReader reader => EnumerateReaderRows(reader, sourcePath, scalar),
            IEnumerable enumerable when sourceValue is not string => EnumerateSourceValues(enumerable, scalar),
            _ => throw new FormatException("The source returned a scalar value. Expected an IEnumerable or IDataReader.")
        };

    private static IEnumerable<object?> EnumerateSourceValues(IEnumerable sourceValue, bool scalar)
    {
        if (scalar)
            throw new FormatException("The --scalar option requires a tabular source exposing exactly one column.");

        var enumerator = sourceValue.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private static IEnumerable<object?> EnumerateReaderRows(IDataReader reader, string sourcePath, bool scalar)
    {
        var headersAreRows = reader is IHeaderDataReader { HeadersAreRows: true };

        if (headersAreRows)
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

                yield return scalar ? GetValue(reader, 0) : BuildRecordValue(reader);
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
            var headers = ReadCsvHeaders(reader, sourcePath);
            var expectedFields = headers.Length;
            ValidateScalarColumnCount(expectedFields, sourcePath, scalar);

            var recordNumber = 2;
            while (true)
            {
                var values = ReadCsvValues(reader, sourcePath, recordNumber, expectedFields);
                if (values is null)
                    yield break;

                yield return scalar ? values[0] : BuildRecordValue(headers, values);
                recordNumber++;
            }
        }
        finally
        {
            reader.Dispose();
        }
    }

    private static string[] ReadCsvHeaders(IDataReader reader, string sourcePath)
    {
        if (!ReadCsvRecord(reader, sourcePath) || reader.FieldCount == 0)
            throw new FormatException($"CSV source '{sourcePath}' is empty. A header row is required.");

        var headers = new string[reader.FieldCount];
        var headerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Length; i++)
        {
            var header = Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture) ?? string.Empty;
            if (string.IsNullOrEmpty(header))
                throw new FormatException($"CSV header in '{sourcePath}' is invalid: field {i + 1} is empty.");

            if (!headerSet.Add(header))
                throw new FormatException($"CSV header in '{sourcePath}' contains duplicate column name '{header}'.");

            headers[i] = header;
        }

        return headers;
    }

    private static object?[]? ReadCsvValues(IDataReader reader, string sourcePath, int recordNumber, int expectedFields)
    {
        if (!ReadCsvRecord(reader, sourcePath))
            return null;

        var actualFields = reader.FieldCount;
        if (actualFields != expectedFields)
            throw new FormatException($"CSV record {recordNumber} in '{sourcePath}' contains {actualFields} fields, but {expectedFields} fields were expected.");

        var values = new object?[expectedFields];
        for (var i = 0; i < expectedFields; i++)
            values[i] = GetValue(reader, i);

        return values;
    }

    private static bool ReadCsvRecord(IDataReader reader, string sourcePath)
    {
        try
        {
            return reader.Read();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            throw new FormatException($"Invalid CSV syntax in '{sourcePath}': {exception.Message}", exception);
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
    private static RecordValue BuildRecordValue(IDataReader reader)
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

        return BuildRecordValue(names, values);
    }

    private static RecordValue BuildRecordValue(IReadOnlyList<string> names, IReadOnlyList<object?> values)
    {
        var record = new RecordValue();
        for (var i = 0; i < names.Count; i++)
            record.Set(names[i], values[i]);

        return record;
    }

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

    internal IDataReader OpenCsvDataReader(string sourcePath, IReadOnlyList<string> sourceOptions)
    {
        FileStream? stream = null;
        try
        {
            stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var (profile, headersAreRows) = BuildCsvProfile(sourceOptions);
            // Expressif builds dynamic row records from the CSV header. PocketCsvReader's
            // schema-driven header mode requires a named schema, so keep header rows visible
            // to the row adapter while retaining the configured profile for validation.
            var readerProfile = profile.Dialect.Header ? WithoutCsvHeaderConsumption(profile) : profile;
            var csvReader = new CsvReader(readerProfile);
            var csvDataReader = csvReader.ToDataReader(stream);
            return new DisposableDataReader(csvDataReader, stream, headersAreRows);
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }

    internal (CsvProfile Profile, bool HeadersAreRows) BuildCsvProfile(IReadOnlyList<string> options)
    {
        var baseline = CsvProfile.CommaDoubleQuote;
        var defaults = baseline.Dialect;
        var header = defaults.Header;
        var headerRows = defaults.HeaderRows;
        var headerJoin = defaults.HeaderJoin;
        var headerRepeat = defaults.HeaderRepeat;
        var commentRows = defaults.CommentRows;
        var commentChar = defaults.CommentChar;
        var delimiter = defaults.Delimiter;
        var lineTerminator = defaults.LineTerminator;
        var quoteChar = defaults.QuoteChar;
        var doubleQuote = defaults.DoubleQuote;
        var escapeChar = defaults.EscapeChar;
        var nullSequence = defaults.NullSequence;
        var missingCell = defaults.MissingCell;
        var skipInitialSpace = defaults.SkipInitialSpace;
        var arrayDelimiter = defaults.ArrayDelimiter;
        var arrayPrefix = defaults.ArrayPrefix;
        var arraySuffix = defaults.ArraySuffix;
        var headersAreRows = true;

        foreach (var option in options)
        {
            var separator = option.IndexOf('=');
            if (separator <= 0)
                throw new FormatException($"Invalid source option '{option}'. Expected <name>=<value>.");

            var name = option[..separator].Trim();
            var suppliedValue = option[(separator + 1)..];
            object? value;
            try
            {
                value = values.ParseStrict(suppliedValue);
            }
            catch (FormatException exception)
            {
                throw InvalidSourceOption(name, suppliedValue, exception.Message);
            }

            try
            {
                switch (name)
                {
                    case "delimiter": delimiter = RequiredChar(value, name); break;
                    case "line-terminator": lineTerminator = RequiredText(value, name); break;
                    case "quote-char": quoteChar = OptionalChar(value, name); break;
                    case "double-quote": doubleQuote = RequiredBoolean(value, name); break;
                    case "escape-char": escapeChar = OptionalChar(value, name); break;
                    case "header": header = headersAreRows = RequiredBoolean(value, name); break;
                    case "header-rows": headerRows = RequiredRows(value, name); break;
                    case "header-join": headerJoin = RequiredText(value, name); break;
                    case "header-repeat": headerRepeat = RequiredBoolean(value, name); break;
                    case "comment-char": commentChar = OptionalChar(value, name); break;
                    case "comment-rows": commentRows = RequiredRows(value, name); break;
                    case "null-sequence": nullSequence = RequiredText(value, name); break;
                    case "missing-cell": missingCell = RequiredText(value, name); break;
                    case "skip-initial-space": skipInitialSpace = RequiredBoolean(value, name); break;
                    case "array-delimiter": arrayDelimiter = OptionalChar(value, name); break;
                    case "array-prefix": arrayPrefix = OptionalChar(value, name); break;
                    case "array-suffix": arraySuffix = OptionalChar(value, name); break;
                    default: throw new FormatException($"Unknown CSV source option '{name}' with value '{suppliedValue}'.");
                }
            }
            catch (FormatException exception) when (!exception.Message.Contains("with value", StringComparison.Ordinal))
            {
                throw InvalidSourceOption(name, suppliedValue, exception.Message);
            }
        }

        CsvProfile profile;
        try
        {
            var dialect = new DialectDescriptor(
                header, headerRows, headerJoin, headerRepeat, commentRows, commentChar,
                delimiter, lineTerminator, quoteChar, doubleQuote, escapeChar, nullSequence,
                missingCell, skipInitialSpace, arrayDelimiter, arrayPrefix, arraySuffix);
            profile = new CsvProfile(dialect, baseline.Schema, baseline.Resource, baseline.Parsers);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            throw new FormatException($"Invalid CSV source-option combination: {exception.Message}", exception);
        }

        return (profile, headersAreRows);
    }

    private static CsvProfile WithoutCsvHeaderConsumption(CsvProfile profile)
    {
        var dialect = profile.Dialect;
        var readerDialect = new DialectDescriptor(
            false, [], dialect.HeaderJoin, dialect.HeaderRepeat, dialect.CommentRows, dialect.CommentChar,
            dialect.Delimiter, dialect.LineTerminator, dialect.QuoteChar, dialect.DoubleQuote,
            dialect.EscapeChar, dialect.NullSequence, dialect.MissingCell, dialect.SkipInitialSpace,
            dialect.ArrayDelimiter, dialect.ArrayPrefix, dialect.ArraySuffix);
        return new CsvProfile(readerDialect, profile.Schema, profile.Resource, profile.Parsers);
    }

    private static FormatException InvalidSourceOption(string name, string value, string reason)
        => new($"Invalid CSV source option '{name}' with value '{value}': {reason}");

    private static bool RequiredBoolean(object? value, string name)
        => value is bool result ? result : throw new FormatException($"'{name}' requires a boolean.");

    private static string RequiredText(object? value, string name)
        => value is string result ? result : throw new FormatException($"'{name}' requires text.");

    private static char RequiredChar(object? value, string name)
        => OptionalChar(value, name) ?? throw new FormatException($"'{name}' cannot be null.");

    private static char? OptionalChar(object? value, string name)
    {
        if (value is null)
            return null;
        if (value is string { Length: 1 } text)
            return text[0];
        throw new FormatException($"'{name}' requires a single character or null.");
    }

    private static int[] RequiredRows(object? value, string name)
    {
        if (value is not object?[] values || values.Length == 0)
            throw new FormatException($"'{name}' requires a non-empty array of one-based row indexes.");

        var rows = new int[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            var row = values[i] switch
            {
                int integer => integer,
                decimal numeric when numeric == decimal.Truncate(numeric)
                                     && numeric <= int.MaxValue
                                     && numeric >= int.MinValue => (int)numeric,
                _ => 0,
            };
            if (row < 1)
                throw new FormatException($"'{name}' requires a non-empty array of one-based row indexes.");
            rows[i] = row;
        }
        return rows;
    }

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

    private static string FormatValue(object? value)
        => ValueFormatter.Format(value);

    private interface IHeaderDataReader : IDataReader
    {
        bool HeadersAreRows { get; }
    }

    private sealed class DisposableDataReader : IHeaderDataReader
    {
        private readonly IDataReader inner;
        private readonly IDisposable owner;

        public DisposableDataReader(IDataReader inner, IDisposable owner, bool headersAreRows = false)
        {
            this.inner = inner;
            this.owner = owner;
            HeadersAreRows = headersAreRows;
        }

        public bool HeadersAreRows { get; }

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
