using Expressif.Parsers;
using Expressif.Predicates.Operators;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Expressif.Functions;

public class ExpressionFactory : BaseExpressionFactory
{
    private Parser<ExpressionMeta> Parser { get; } = ExpressionParser.Parser;

    public ExpressionFactory()
        : base(new FunctionTypeMapper()) { }

    public IFunction Instantiate(string code, IContext context)
    {
        var expression = Parser.Parse(code);

        var functions = new List<IFunction>();
        foreach (var member in expression.Members)
            functions.Add(Instantiate<IFunction>(member.Name, member.Parameters, context));
        return new ChainOperator(functions);
    }

    public IFunction Instantiate(string name, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(name, parameters, context);

    public IFunction Instantiate(Type type, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(type, parameters, context);
}
