using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Parsers;

namespace Expressif.Predicates.Operators;

public interface IOperatedStructure
{ }

public record Single<T>(T Member) : IOperatedStructure
{ }

public record Unary<T>(IUnaryOperator @Operator, T Member) : IOperatedStructure
{ }

public record Binary<T>(IBinaryOperator @Operator, T Left, T Right) : IOperatedStructure
{ }
