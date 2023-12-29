using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif;

public class Expression : IFunction
{
    private readonly IFunction expression;
    public Expression(string code)
        : this(code, new Context()) { }
    public Expression(string code, IContext context)
        : this(code, context, new ExpressionFactory()) { }
    public Expression(string code, IContext context, ExpressionFactory factory)
        => expression = factory.Instantiate(code, context);

    public object? Evaluate(object? value) => expression.Evaluate(value);
}
