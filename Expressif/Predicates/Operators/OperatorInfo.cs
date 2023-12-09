using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Operators;
public record OperatorInfo
(
    string Name,
    bool IsPublic,
    string[] Aliases,
    Type ImplementationType,
    string Summary
);
