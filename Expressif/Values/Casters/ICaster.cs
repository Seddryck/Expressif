using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public interface ICaster<T>
    {
        bool TryCast(object obj, [NotNullWhen(true)] out T? value);
        T Cast(object obj);
    }

    public interface IParser<T>
    {
        bool TryParse(string text, [NotNullWhen(true)] out T? value);
        T Parse(string text);
    }
}
