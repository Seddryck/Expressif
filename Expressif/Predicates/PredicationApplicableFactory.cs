using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Predicates.Operators;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Expressif.Predicates;

public class PredicationApplicableFactory
{
    protected Parser<PredicationApplicableMeta> Parser { get; }
    protected PredicationFactory Factory { get; }

    protected internal PredicationApplicableFactory(PredicationFactory? factory = null, Parser<PredicationApplicableMeta>? parser = null)
        => (Factory, Parser) = (factory ?? new(), parser ?? PredicationApplicableParser.Parser);

    public virtual PredicationApplicable Instantiate(string code, IContext context)
        => Instantiate(Parser.Parse(code), context);

    internal virtual PredicationApplicable Instantiate(PredicationApplicableMeta applicable, IContext context)
    {
        var predicate = Factory.Instantiate(applicable.Member, context);
        return new (applicable.Argument, predicate, context);
    }
}
