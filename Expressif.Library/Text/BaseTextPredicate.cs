using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Library.Text;

public abstract class BaseTextPredicate : BasePredicate
{
    public override bool Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            DBNull => EvaluateNull(),
            string text => EvaluateBaseText(text),
            _ => EvaluateUncasted(value),
        };
    }

    protected abstract bool EvaluateBaseText(string value);

    protected virtual bool EvaluateUncasted(object value)
    {
        if (Expressif.Values.Special.Null.Instance.Equals(value))
            return EvaluateNull();
        if (Expressif.Values.Special.Empty.Instance.Equals(value))
            return EvaluateBaseText(string.Empty);

        var caster = new TextCaster();
        var text = caster.Cast(value);
        return EvaluateBaseText(text);
    }
}

public abstract class BaseTextPredicateWithoutReference : BaseTextPredicate
{
    protected override bool EvaluateBaseText(string value)
    {
        if (Values.Special.Null.Instance.Equals(value))
            return EvaluateNull();

        if (Values.Special.Empty.Instance.Equals(value))
            return EvaluateText(string.Empty);

        return EvaluateText(value);
    }
    protected abstract bool EvaluateText(string value);
}

public abstract class BaseTextPredicateReference : BaseTextPredicate
{
    public Func<string?> Reference { get; }

    public BaseTextPredicateReference(Func<string?> reference)
        => Reference = reference;

    protected override bool EvaluateBaseText(string value)
    {
        if (Expressif.Values.Special.Null.Instance.Equals(value))
            return EvaluateNull();

        var reference = Reference.Invoke();
        if (Expressif.Values.Special.Null.Instance.Equals(reference))
            return EvaluateNull(reference);
        if ((Whitespace.Instance.Equals(value) || Whitespace.Instance.Equals(reference))
            && !(Values.Special.Empty.Instance.Equals(value) || Values.Special.Empty.Instance.Equals(reference)))
            return EvaluateWhitespaces();

        if (Values.Special.Empty.Instance.Equals(value))
            value = string.Empty;

        if (Values.Special.Empty.Instance.Equals(reference))
            reference = string.Empty;

        return EvaluateText(value, reference!);
    }

    protected virtual bool EvaluateNull(string? reference) => EvaluateNull();
    protected abstract bool EvaluateText(string value, string reference);
    protected virtual bool EvaluateWhitespaces() => false;
}
