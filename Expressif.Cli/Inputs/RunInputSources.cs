using System.Collections;
using Expressif.Values;

namespace Expressif.Cli.Inputs;

internal interface IRunInputSource
{
    IEnumerable<object?> Read();
}

internal sealed class RepeatedInputSource(IEnumerable<string> rows, IInputValueParser parser) : IRunInputSource
{
    public IEnumerable<object?> Read()
    {
        foreach (var row in rows)
        {
            object? value;
            try { value = parser.Parse(row); }
            catch (FormatException exception)
            { throw new FormatException($"Invalid input syntax for --input '{row}': {exception.Message}", exception); }
            yield return value;
        }
    }
}

internal sealed class BatchInputSource(string? input, IInputValueParser parser) : IRunInputSource
{
    public IEnumerable<object?> Read()
    {
        object? value;
        try { value = parser.Parse(input ?? string.Empty); }
        catch (FormatException exception)
        { throw new FormatException($"Invalid input syntax for --batch '{input}': {exception.Message}", exception); }

        if (value is not IEnumerable enumerable || value is string or RecordValue)
            throw new FormatException("The --batch option requires an enumerable value.");

        var enumerator = enumerable.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        finally { (enumerator as IDisposable)?.Dispose(); }
    }
}

internal sealed class CompositeInputSource(params IRunInputSource[] sources) : IRunInputSource
{
    public IEnumerable<object?> Read()
    {
        foreach (var source in sources)
        {
            foreach (var row in source.Read())
            {
                yield return row;
            }
        }
    }
}
