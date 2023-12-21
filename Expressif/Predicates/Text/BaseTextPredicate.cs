using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text;

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
        if (new Null().Equals(value))
            return EvaluateNull();
        if (new Empty().Equals(value))
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
        if (new Values.Special.Null().Equals(value))
            return EvaluateNull();

        if (new Values.Special.Empty().Equals(value))
            return EvaluateText(string.Empty);
        
        return EvaluateText(value);
    }
    protected abstract bool EvaluateText(string value);
}

public abstract class BaseTextPredicateReference : BaseTextPredicate
{
    public Func<string> Reference { get; }

    public BaseTextPredicateReference(Func<string> reference)
        => Reference = reference;
    
    protected override bool EvaluateBaseText(string value)
    {
        if (new Values.Special.Null().Equals(value) || new Values.Special.Null().Equals(Reference.Invoke()))
            return EvaluateNull();
        if ((new Values.Special.Whitespace().Equals(value) || new Values.Special.Whitespace().Equals(Reference.Invoke()))
            && !(new Values.Special.Empty().Equals(value) || new Values.Special.Empty().Equals(Reference.Invoke())))
            return EvaluateWhitespaces();

        if (new Values.Special.Empty().Equals(value))
            value = string.Empty;

        var reference = Reference.Invoke()!;
        if (new Values.Special.Empty().Equals(reference))
            reference = string.Empty;

        return EvaluateText(value, reference);
    }

    protected abstract bool EvaluateText(string value, string reference);
    protected virtual bool EvaluateWhitespaces() => false;
}