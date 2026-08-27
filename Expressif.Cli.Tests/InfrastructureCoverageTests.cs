using System.Collections;
using System.Data;
using Expressif.Cli.Expressions;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;

namespace Expressif.Cli.Tests;

public class InfrastructureCoverageTests
{
    [Test]
    public void Normalize_EnumerableInScalarMode_RejectsNonTabularSource()
    {
        var infrastructure = CreateInfrastructure();

        Assert.That(
            () => infrastructure.Normalize(new[] { 1 }, "source.expr", scalar: true).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("tabular source"));
    }

    [Test]
    public void Normalize_Enumerable_DisposesEnumerator()
    {
        var source = new TrackingEnumerable();

        Assert.That(CreateInfrastructure().Normalize(source, "source.expr").ToArray(), Is.EqualTo(new[] { 1 }));
        Assert.That(source.IsDisposed, Is.True);
    }

    [Test]
    public void Normalize_MultiColumnReaderInScalarMode_ReportsColumnCount()
    {
        var table = new DataTable();
        table.Columns.Add("first");
        table.Columns.Add("second");
        using var reader = table.CreateDataReader();

        Assert.That(
            () => CreateInfrastructure().Normalize(reader, "source.sql", scalar: true).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("exactly one column; found 2"));
    }

    [Test]
    public void Normalize_ReaderWithDuplicateColumnNames_RejectsDuplicate()
    {
        var table = new DataTable();
        table.Columns.Add("first");
        table.Columns.Add("second");
        table.Rows.Add("alpha", "beta");
        using var reader = new DuplicateNameDataReader(table.CreateDataReader());

        Assert.That(
            () => CreateInfrastructure().Normalize(reader, "source.sql").ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("duplicate column name 'name'"));
    }

    [Test]
    public void Normalize_CsvReaderWithEmptyHeader_ReportsFieldPosition()
    {
        var table = new DataTable();
        table.Columns.Add("column");
        table.Rows.Add(string.Empty);
        using var inner = table.CreateDataReader();
        using var reader = new OwnedDataReader(inner, new TrackingDisposable(), headersAreRows: true);

        Assert.That(
            () => CreateInfrastructure().Normalize(reader, "source.csv").ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("field 1 is empty"));
    }

    [Test]
    public void OpenExpressionSource_WithOptions_RejectsUnsupportedOptions()
        => Assert.That(
            () => CreateInfrastructure().OpenExpressionSource("source.expr", ["mode=fast"]),
            Throws.TypeOf<FormatException>().With.Message.Contains("not supported"));

    [Test]
    public void OpenExpressionSource_CompilationFailure_IsSourceAware()
    {
        var expressions = new FakeExpressionService
        {
            CompileClosedHandler = static (_, _) => throw new NotImplementedFunctionException("missing"),
        };

        Assert.That(
            () => CreateInfrastructure(expressions).OpenExpressionSource("source.expr", []),
            Throws.TypeOf<FormatException>().With.Message.Contains("source.expr").And.Message.Contains("Unknown function 'missing'"));
    }

    [Test]
    public void OpenExpressionSource_EvaluationFailure_IsSourceAware()
    {
        var expressions = new FakeExpressionService
        {
            EvaluateHandler = static (_, _) => throw new InvalidOperationException("boom"),
        };

        Assert.That(
            () => CreateInfrastructure(expressions).OpenExpressionSource("source.expr", []),
            Throws.TypeOf<FormatException>().With.Message.Contains("could not be evaluated: boom"));
    }

    [TestCase((int)TextFileFailureKind.InvalidUtf8, "decoded as UTF-8")]
    [TestCase((int)TextFileFailureKind.Access, "could not be accessed")]
    public void OpenExpressionSource_TextReadFailure_IsNormalized(int kind, string expected)
    {
        var textFiles = new FakeTextReader
        {
            ReadHandler = path => throw new TextFileReadException(
                path,
                (TextFileFailureKind)kind,
                new IOException("failure")),
        };

        Assert.That(
            () => CreateInfrastructure(textFiles: textFiles).OpenExpressionSource("source.expr", []),
            Throws.TypeOf<FormatException>().With.Message.Contains(expected));
    }

    [Test]
    public void SourcePipeline_WithoutMatchingProvider_ReportsPath()
    {
        var infrastructure = CreateInfrastructure();
        var pipeline = new SourcePipeline([], infrastructure);

        Assert.That(
            () => pipeline.Read("virtual.source", []).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("No source provider"));
    }

    [Test]
    public void SourcePipeline_ProviderFailure_IsNormalized()
    {
        var infrastructure = CreateInfrastructure();
        var pipeline = new SourcePipeline([new ThrowingSourceProvider()], infrastructure);

        Assert.That(
            () => pipeline.Read("virtual.source", []).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("could not be resolved: boom"));
    }

    [Test]
    public void SourcePathValidator_RejectsBlankDirectoryAndMissingPaths()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => SourcePathValidator.Validate(" "), Throws.TypeOf<FormatException>());
            Assert.That(() => SourcePathValidator.Validate(Path.GetTempPath()), Throws.TypeOf<FormatException>());
            Assert.That(
                () => SourcePathValidator.Validate(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}")),
                Throws.TypeOf<FormatException>());
        });
    }

    [Test]
    public void InputSources_ParserFailures_IdentifyTheirOption()
    {
        var parser = new ThrowingInputValueParser();

        Assert.Multiple(() =>
        {
            Assert.That(
                () => new RepeatedInputSource(["bad"], parser).Read().ToArray(),
                Throws.TypeOf<FormatException>().With.Message.Contains("--input 'bad'"));
            Assert.That(
                () => new BatchInputSource("bad", parser).Read().ToArray(),
                Throws.TypeOf<FormatException>().With.Message.Contains("--batch 'bad'"));
        });
    }

    [Test]
    public void ExpressionService_CompilesAndEvaluatesOpenAndClosedExpressions()
    {
        var service = new ExpressionService();
        var context = new Context();

        Assert.Multiple(() =>
        {
            Assert.That(service.Evaluate(service.CompileOpen("add(1)", context), 2), Is.EqualTo(3));
            Assert.That(service.Evaluate(service.CompileClosed("1 | add(2)", context), null), Is.EqualTo(3));
        });
    }

    private static SourceInfrastructure CreateInfrastructure(
        IExpressionService? expressions = null,
        IStrictUtf8TextReader? textFiles = null)
        => new(
            expressions ?? new FakeExpressionService(),
            new CliInputValueParser(),
            textFiles ?? new FakeTextReader());

    private sealed class FakeExpressionService : IExpressionService
    {
        public Func<string, Context, IExpression> CompileClosedHandler { get; init; }
            = static (code, context) => Expression.CreateClosed(code, context);

        public Func<IExpression, object?, object?> EvaluateHandler { get; init; }
            = static (expression, input) => expression.Evaluate(input);

        public IExpression CompileOpen(string code, Context context) => Expression.Create(code, context);
        public IExpression CompileClosed(string code, Context context) => CompileClosedHandler(code, context);
        public object? Evaluate(IExpression expression, object? input) => EvaluateHandler(expression, input);
    }

    private sealed class FakeTextReader : IStrictUtf8TextReader
    {
        public Func<string, string> ReadHandler { get; init; } = static _ => "{1, 2}";

        public string Read(string path, bool requireContent = true) => ReadHandler(path);
    }

    private sealed class ThrowingSourceProvider : IFileSourceProvider
    {
        public bool CanOpen(string path) => true;
        public object? Open(string path, IReadOnlyList<string> options) => throw new InvalidOperationException("boom");
    }

    private sealed class ThrowingInputValueParser : IInputValueParser
    {
        public object? Parse(string text) => throw new FormatException("invalid syntax");
        public object? ParseStrict(string text) => throw new FormatException("invalid syntax");
    }

    private sealed class TrackingEnumerable : IEnumerable
    {
        public bool IsDisposed { get; private set; }

        public IEnumerator GetEnumerator() => new Enumerator(this);

        private sealed class Enumerator(TrackingEnumerable owner) : IEnumerator, IDisposable
        {
            private bool beforeFirst = true;

            public object Current => 1;

            public bool MoveNext()
            {
                if (!beforeFirst)
                    return false;
                beforeFirst = false;
                return true;
            }

            public void Reset() => beforeFirst = true;
            public void Dispose() => owner.IsDisposed = true;
        }
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class DuplicateNameDataReader(IDataReader inner) : IDataReader
    {
        public object this[int i] => inner[i];
        public object this[string name] => inner[name];
        public int Depth => inner.Depth;
        public bool IsClosed => inner.IsClosed;
        public int RecordsAffected => inner.RecordsAffected;
        public int FieldCount => inner.FieldCount;
        public void Close() => inner.Close();
        public void Dispose() => inner.Dispose();
        public bool GetBoolean(int i) => inner.GetBoolean(i);
        public byte GetByte(int i) => inner.GetByte(i);
        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => inner.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        public char GetChar(int i) => inner.GetChar(i);
        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => inner.GetChars(i, fieldoffset, buffer, bufferoffset, length);
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
        public string GetName(int i) => "name";
        public int GetOrdinal(string name) => inner.GetOrdinal(name);
        public DataTable? GetSchemaTable() => inner.GetSchemaTable();
        public string GetString(int i) => inner.GetString(i);
        public object GetValue(int i) => inner.GetValue(i);
        public int GetValues(object[] values) => inner.GetValues(values);
        public bool IsDBNull(int i) => inner.IsDBNull(i);
        public bool NextResult() => inner.NextResult();
        public bool Read() => inner.Read();
    }
}
