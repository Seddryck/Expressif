using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Parsers;

namespace Expressif.Functions;

public record FunctionMeta(string Name, IParameter[] Parameters) : IExpressionParsable
{ }

public record ExpressionMeta(FunctionMeta[] Members)
{ }

public record ExpressionApplicableMeta(IParameter Parameter, FunctionMeta[] Members)
{ }

