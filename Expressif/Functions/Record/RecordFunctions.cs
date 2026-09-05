using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Expressif.Values;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Functions.Record;

/// <summary>
/// Returns the value of the named field from the input record or object.
/// Returns <see langword="null"/> when the field does not exist or the input does not expose named values.
/// </summary>
[Function(prefix: "")]
public class Field : IFunction
{
    private Func<string> Name { get; }

    /// <param name="name">Name of the field to retrieve from the input.</param>
    public Field(Func<string> name)
        => Name = name;

    public object? Evaluate(object? value)
        => NamedValueAccessor.TryGetValue(value, Name.Invoke(), out var result) ? result : null;
}

/// <summary>
/// Creates a record by evaluating its named and spread entries against the input value.
/// Later entries overwrite fields with the same name created by earlier entries.
/// </summary>
[Function(prefix: "")]
public class Record : IFunction<object?, ValueRecord>, IValueSpreadAware
{
    private Func<RecordEntryEvaluator[]> Entries { get; }

    /// <summary>Creates an empty record constructor.</summary>
    public Record()
        : this(() => []) { }

    /// <param name="entries">Zero or more named or spread entries used to construct the resulting record. Each entry is evaluated against the input value.</param>
    public Record(Func<RecordEntryEvaluator[]> entries)
        => Entries = entries;

    public object? Evaluate(object? value)
    {
        var record = new ValueRecord();
        foreach (var entry in Entries.Invoke())
            entry.Apply(value, record);

        return record;
    }

    ValueRecord IFunction<object?, ValueRecord>.Evaluate(object? value) => (ValueRecord)Evaluate(value)!;
}

/// <summary>
/// Evaluates named projections against the same input and evaluates a body expression against their temporary record.
/// </summary>
[Function(prefix: "")]
public class With : IFunction
{
    private Func<RecordEntryEvaluator[]> Projections { get; }
    private Func<object?, object?> Body { get; }

    /// <param name="projections">One or more named projections evaluated independently against the input value.</param>
    /// <param name="body">The final expression evaluated against the temporary projection record.</param>
    public With(Func<RecordEntryEvaluator[]> projections, Func<object?, object?> body)
        => (Projections, Body) = (projections, body);

    public object? Evaluate(object? value)
    {
        var temporary = new ValueRecord();
        foreach (var projection in Projections.Invoke())
            projection.Apply(value, temporary);

        using var scope = EvaluationRuntime.Derive(temporary);
        return Body.Invoke(temporary);
    }
}

public enum AssignmentMode
{
    Always,
    Present,
    Absent,
}

internal static class AssignmentModeExtensions
{
    public static bool Applies(this AssignmentMode mode, bool present)
        => mode is AssignmentMode.Always
            || (mode is AssignmentMode.Present && present)
            || (mode is AssignmentMode.Absent && !present);
}

public sealed record RecordAssignmentEvaluator(string Name, Func<object?, object?> Evaluator);

/// <summary>Creates or replaces statically named fields while preserving the other fields of the input record.</summary>
public abstract class BasePut : IFunction<ValueRecord, ValueRecord>
{
    private Func<RecordAssignmentEvaluator[]> Assignments { get; }
    private AssignmentMode Mode { get; }

    protected BasePut(Func<RecordAssignmentEvaluator[]> assignments, AssignmentMode mode)
        => (Assignments, Mode) = (assignments, mode);

    public ValueRecord Evaluate(object? input)
    {
        var result = RecordOperations.Copy(input);
        foreach (var assignment in Assignments.Invoke())
        {
            var present = result.ContainsKey(assignment.Name);
            if (Mode.Applies(present))
                result.Set(assignment.Name, assignment.Evaluator.Invoke(input));
        }
        return result;
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);

    ValueRecord IFunction<ValueRecord, ValueRecord>.Evaluate(ValueRecord value) => Evaluate(value);
}

/// <summary>Creates or replaces statically named fields while preserving every other field. Assignment expressions are evaluated against the original input record.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class Put : BasePut
{
    /// <param name="assignments">One or more named assignments evaluated against the original input record.</param>
    public Put(Func<RecordAssignmentEvaluator[]> assignments)
        : base(assignments, AssignmentMode.Always) { }
}

/// <summary>Assigns statically named fields only when they are present, including fields whose value is null.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class PutPresent : BasePut
{
    /// <param name="assignments">One or more named assignments applied only to fields already present.</param>
    public PutPresent(Func<RecordAssignmentEvaluator[]> assignments)
        : base(assignments, AssignmentMode.Present) { }
}

/// <summary>Assigns statically named fields only when they are absent; a present field containing null remains unchanged.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class PutAbsent : BasePut
{
    /// <param name="assignments">One or more named assignments applied only to fields that are absent.</param>
    public PutAbsent(Func<RecordAssignmentEvaluator[]> assignments)
        : base(assignments, AssignmentMode.Absent) { }
}

/// <summary>Creates or replaces a field at a dynamically evaluated record path.</summary>
public abstract class BasePutPath : IFunction<ValueRecord, ValueRecord>
{
    private Func<object?, object?> Path { get; }
    private Func<object?, object?> Value { get; }
    private AssignmentMode Mode { get; }

    /// <param name="path">An expression producing non-empty text for one literal segment or a non-empty tuple of non-empty text segments.</param>
    /// <param name="value">The expression producing the assigned value from the original input record.</param>
    /// <param name="mode">The target-presence condition for assignment.</param>
    protected BasePutPath(Func<object?, object?> path, Func<object?, object?> value, AssignmentMode mode)
        => (Path, Value, Mode) = (path, value, mode);

    public ValueRecord Evaluate(object? input)
    {
        var segments = RecordOperations.ResolvePath(Path.Invoke(input));
        var assigned = Value.Invoke(input);
        return RecordOperations.AssignPath(input, segments, assigned, Mode);
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);

    ValueRecord IFunction<ValueRecord, ValueRecord>.Evaluate(ValueRecord value) => Evaluate(value);
}

/// <summary>Creates or replaces the field at a dynamic path. Text is one literal segment; a tuple supplies nested segments, creating missing intermediate records.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class PutPath : BasePutPath
{
    /// <param name="path">An expression producing non-empty text for one literal segment or a non-empty tuple of non-empty text segments.</param>
    /// <param name="value">The expression producing the assigned value from the original input record.</param>
    public PutPath(Func<object?, object?> path, Func<object?, object?> value)
        : base(path, value, AssignmentMode.Always) { }
}

/// <summary>Assigns the field at a dynamic path only when the final segment is present, including when its value is null.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class PutPresentPath : BasePutPath
{
    /// <param name="path">An expression producing non-empty text for one literal segment or a non-empty tuple of non-empty text segments.</param>
    /// <param name="value">The expression producing the assigned value from the original input record.</param>
    public PutPresentPath(Func<object?, object?> path, Func<object?, object?> value)
        : base(path, value, AssignmentMode.Present) { }
}

/// <summary>Assigns the field at a dynamic path only when the final segment is absent, creating missing intermediate records.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class PutAbsentPath : BasePutPath
{
    /// <param name="path">An expression producing non-empty text for one literal segment or a non-empty tuple of non-empty text segments.</param>
    /// <param name="value">The expression producing the assigned value from the original input record.</param>
    public PutAbsentPath(Func<object?, object?> path, Func<object?, object?> value)
        : base(path, value, AssignmentMode.Absent) { }
}

/// <summary>Removes null-valued fields from the input record without traversing nested records or collections.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class DropNullFields : IFunction<ValueRecord, ValueRecord>
{
    public ValueRecord Evaluate(object? input)
    {
        var result = new ValueRecord();
        foreach (var field in RecordOperations.Enumerate(input))
        {
            if (field.Value is not null)
                result.Set(field.Key, field.Value);
        }
        return result;
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);

    ValueRecord IFunction<ValueRecord, ValueRecord>.Evaluate(ValueRecord value) => Evaluate(value);
}

internal static class RecordOperations
{
    internal static IReadOnlyList<KeyValuePair<string, object?>> Enumerate(object? input)
        => NamedValueAccessor.TryEnumerate(input, out var fields)
            ? fields
            : throw new ArgumentException("Input value must be a record.", nameof(input));

    internal static ValueRecord Copy(object? input)
    {
        var copy = new ValueRecord();
        foreach (var field in Enumerate(input))
            copy.Set(field.Key, field.Value);
        return copy;
    }

    internal static string[] ResolvePath(object? value)
    {
        if (value is string text)
        {
            return string.IsNullOrEmpty(text)
                ? throw new ArgumentException("Record path segments must not be empty.", nameof(value))
                : [text];
        }
        if (value is not TupleValue tuple)
            throw new ArgumentException("Record path must evaluate to text or a tuple of text segments.", nameof(value));
        if (tuple.Count == 0)
            throw new ArgumentException("Record path tuple must contain at least one segment.", nameof(value));
        if (tuple.Any(segment => segment is not string))
            throw new ArgumentException("Every record path tuple segment must be text.", nameof(value));
        if (tuple.Cast<string>().Any(string.IsNullOrEmpty))
            throw new ArgumentException("Record path segments must not be empty.", nameof(value));
        return tuple.Cast<string>().ToArray();
    }

    internal static ValueRecord AssignPath(object? input, IReadOnlyList<string> path, object? value, AssignmentMode mode)
    {
        var result = Copy(input);
        Assign(result, path, 0, value, mode);
        return result;
    }

    private static bool Assign(ValueRecord parent, IReadOnlyList<string> path, int index, object? value, AssignmentMode mode)
    {
        var segment = path[index];
        if (index == path.Count - 1)
        {
            if (mode.Applies(parent.ContainsKey(segment)))
            {
                parent.Set(segment, value);
                return true;
            }
            return false;
        }

        ValueRecord child;
        if (!parent.TryGetValue(segment, out var existing))
        {
            if (mode is AssignmentMode.Present)
            {
                return false;
            }
            child = new ValueRecord();
        }
        else
        {
            try
            {
                child = Copy(existing);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException($"Cannot traverse record path through non-record segment '{segment}'.", nameof(path), exception);
            }
        }

        if (!Assign(child, path, index + 1, value, mode))
            return false;
        parent.Set(segment, child);
        return true;
    }
}

public class RecordEntryEvaluator
{
    private string? Name { get; }
    private Func<object?, object?> Evaluator { get; }

    private bool IsSpread => Name == null;

    private RecordEntryEvaluator(string? name, Func<object?, object?> evaluator)
        => (Name, Evaluator) = (name, evaluator);

    public static RecordEntryEvaluator Spread(Func<object?, object?> evaluator)
        => new(null, evaluator);

    public static RecordEntryEvaluator Named(string name, Func<object?, object?> evaluator)
        => new(name, evaluator);

    public void Apply(object? input, ValueRecord target)
    {
        if (IsSpread)
        {
            ApplySpread(Evaluator.Invoke(input), target);
            return;
        }

        target.Set(Name!, Evaluator.Invoke(input));
    }

    private static void ApplySpread(object? input, ValueRecord target)
    {
        if (IsRecord(input)
            && NamedValueAccessor.TryEnumerate(input, out IReadOnlyList<KeyValuePair<string, object?>>? fields))
        {
            foreach (var field in fields)
                target.Set(field.Key, field.Value);

            return;
        }

        throw new SpreadArgumentException("Spread argument must evaluate to a record.");
    }

    private static bool IsRecord(object? value)
        => value is ValueRecord
            or IReadOnlyDictionary<string, object?>
            or IDictionary<string, object?>
            or IDictionary
            or DataRow
            or ILiteDataRow;
}
