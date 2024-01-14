using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif;
public interface IApplicable
{
    object? Evaluate();
}

public interface IPredicationApplicable : IApplicable
{
    new bool Evaluate();
}
