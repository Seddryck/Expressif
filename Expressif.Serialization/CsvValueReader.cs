using System.Data;
using PocketCsvReader;

namespace Expressif.Serialization;

/// <summary>Reads CSV streams as Expressif records or single-column scalar rows.</summary>
public static class CsvValueReader
{
    /// <summary>Enumerates CSV rows. The caller retains ownership of the stream.</summary>
    public static IEnumerable<object?> Read(Stream stream, IReadOnlyList<CsvSourceOption>? options = null, bool scalar = false, string sourceName = "CSV")
    {
        var reader = OpenDataReader(stream, options);
        foreach (var row in SourceRows.Read(reader, sourceName, scalar))
            yield return row;
    }

    /// <summary>Opens a CSV data reader, leaving the stream open by default. When ownership is transferred, the caller must still dispose the stream if opening fails.</summary>
    public static IDataReader OpenDataReader(Stream stream, IReadOnlyList<CsvSourceOption>? options = null, bool leaveOpen = true)
    {
        ArgumentNullException.ThrowIfNull(stream);
        options ??= [];
        var (profile, headersAreRows) = new CsvSourceProfileBuilder().Build(options);
        var useConfiguredHeaderProcessing = options.Any(option => option.Name is "header-rows" or "header-repeat");
        var readerProfile = useConfiguredHeaderProcessing
            ? WithCsvHeaderConsumption(profile)
            : profile.Dialect.Header ? WithoutCsvHeaderConsumption(profile) : profile;
        Stream input = leaveOpen ? new BorrowedStream(stream) : stream;
        try
        {
            var reader = new CsvReader(readerProfile).ToDataReader(input);
            return new OwnedDataReader(reader, input,
                headersAreRows && !useConfiguredHeaderProcessing,
                skipRepeatedHeaders: useConfiguredHeaderProcessing && profile.Dialect.HeaderRepeat);
        }
        catch
        {
            input.Dispose();
            throw;
        }
    }

    private static CsvProfile WithCsvHeaderConsumption(CsvProfile profile)
    {
        var dialect = profile.Dialect;
        var headerRows = dialect.HeaderRows.Length == 0 ? new[] { 1 } : dialect.HeaderRows;
        var readerDialect = new DialectDescriptor(
            true, headerRows, dialect.HeaderJoin, dialect.HeaderRepeat, dialect.CommentRows, dialect.CommentChar,
            dialect.Delimiter, dialect.LineTerminator, dialect.QuoteChar, dialect.DoubleQuote,
            dialect.EscapeChar, dialect.NullSequence, dialect.MissingCell, dialect.SkipInitialSpace,
            dialect.ArrayDelimiter, dialect.ArrayPrefix, dialect.ArraySuffix);
        return new CsvProfile(readerDialect, profile.Schema, profile.Resource, profile.Parsers);
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
}
