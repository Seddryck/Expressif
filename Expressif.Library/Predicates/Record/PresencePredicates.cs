using Expressif.Functions;
using Expressif.Values;

namespace Expressif.Predicates.Record;

/// <summary>Returns whether the named field exists in the input record, independently of its value.</summary>
[Predicate(appendIs: false, prefix: "", name: "is-present")]
[Scope("record")]
public sealed class IsPresent : BasePredicate, IPredicate<RecordValue>
{
    private Func<string> Name { get; }

    /// <param name="name">Name of the field whose presence is tested.</param>
    public IsPresent(Func<string> name) => Name = name;

    public override bool Evaluate(object? value) => NamedValueAccessor.Contains(value, Name.Invoke());

    public bool Evaluate(RecordValue? value) => Evaluate((object?)value);
}

/// <summary>Returns whether the named field does not exist in the input record, independently of field values.</summary>
[Predicate(appendIs: false, prefix: "", name: "is-absent")]
[Scope("record")]
public sealed class IsAbsent : BasePredicate, IPredicate<RecordValue>
{
    private Func<string> Name { get; }

    /// <param name="name">Name of the field whose absence is tested.</param>
    public IsAbsent(Func<string> name) => Name = name;

    public override bool Evaluate(object? value) => !NamedValueAccessor.Contains(value, Name.Invoke());

    public bool Evaluate(RecordValue? value) => Evaluate((object?)value);
}
