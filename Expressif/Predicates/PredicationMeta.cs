using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Predicates.Operators;

namespace Expressif.Predicates;

public interface IPredicationParsable
{ }

public record SinglePredicationMeta(FunctionMeta Member) : IPredicationParsable
{ }

public record UnaryPredicationMeta(OperatorMeta Operator, IPredicationParsable Member) : IPredicationParsable
{ }

public record BinaryPredicationMeta(
    OperatorMeta Operator
    , IPredicationParsable Left
    , IPredicationParsable Right
) : IPredicationParsable
{ }

public record PredicationApplicableMeta(
    IParameter Argument
    , IPredicationParsable Member
) : IPredicationParsable
{ }
