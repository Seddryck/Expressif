using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif;

public class Expression : IExpression
{
    private readonly IFunction expression;

    public static IExpression Create(string text)
        => new ExpressionFactory().Create(text);

    internal Expression(IFunction expression)
        => this.expression = expression;

    public Expression(string code)
        : this(code, new Context()) { }
    public Expression(string code, IContext context)
        : this(code, context, new Functions.FunctionFactory()) { }
    public Expression(string code, IContext context, Functions.FunctionFactory factory)
        => expression = factory.Instantiate(code, context);

    public object? Evaluate(object? value) => expression.Evaluate(value);
}
