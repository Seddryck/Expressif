using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;

namespace Expressif.Functions;
public interface IParameter
{ }

public record class LiteralParameter(string Value) : IParameter { }
public record class IntervalParameter(IntervalMeta Value) : IParameter { }
public record class VariableParameter(string Name) : IParameter { }
public record class ObjectPropertyParameter(string Name) : IParameter { }
public record class ObjectIndexParameter(int Index) : IParameter { }
public record class ContextParameter(Func<IContext, object?> Function) : IParameter { }

public record class InputExpressionParameter(ExpressionApplicableMeta Expression) : IParameter { }
