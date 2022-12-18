using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates
{
    public interface IPredicate : IFunction
    {
        new bool Evaluate(object? value);
    }

    public interface IPredicate<T> : IPredicate
    {
        bool Evaluate(T? value);
    }
}
