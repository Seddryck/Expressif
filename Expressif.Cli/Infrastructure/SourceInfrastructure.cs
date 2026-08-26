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
        if (reader is IHeaderDataReader headerReader)
        {
            var rows = headerReader.HeadersAreRows
                ? EnumerateCsvRows(reader, sourcePath, scalar)
                : EnumerateHeaderlessCsvRows(reader, sourcePath, scalar);
            foreach (var row in rows)
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

    private static IEnumerable<object?> EnumerateHeaderlessCsvRows(IDataReader reader, string sourcePath, bool scalar)
    {
        try
        {
            var expectedFields = 0;
            var recordNumber = 1;
            while (ReadCsvRecord(reader, sourcePath))
            {
                var actualFields = reader.FieldCount;
                if (recordNumber == 1)
                {
                    expectedFields = actualFields;
                    ValidateScalarColumnCount(expectedFields, sourcePath, scalar);
                }
                else if (actualFields != expectedFields)
                {
                    throw new FormatException($"CSV record {recordNumber} in '{sourcePath}' contains {actualFields} fields, but {expectedFields} fields were expected.");
                }

                yield return scalar ? GetValue(reader, 0) : BuildHeaderlessRecordValue(reader);
                recordNumber++;
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
        var nameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < fields; i++)
        {
            names[i] = reader.GetName(i);
            if (!nameSet.Add(names[i]))
                throw new FormatException($"The source contains duplicate column name '{names[i]}'.");

            var value = reader.GetValue(i);
            values[i] = value is DBNull ? null : value;
        }

        return BuildRecordValue(names, values);
    }

    private static RecordValue BuildHeaderlessRecordValue(IDataReader reader)
    {
        var fields = reader.FieldCount;
        var names = new string[fields];
        var values = new object?[fields];
        for (var i = 0; i < fields; i++)
        {
            names[i] = $"column{i + 1}";
            values[i] = GetValue(reader, i);
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
            return new OwnedDataReader(csvDataReader, stream, headersAreRows);
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }

    internal (CsvProfile Profile, bool HeadersAreRows) BuildCsvProfile(IReadOnlyList<string> options)
        => new CsvSourceProfileBuilder(values).Build(options);

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
}
