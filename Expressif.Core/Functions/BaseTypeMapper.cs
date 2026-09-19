using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions;

public abstract class BaseTypeMapper
{
    private IDictionary<string, Type>? mapping;
    protected IDictionary<string, Type> Mapping { get => mapping ??= Initialize(); }

    public Type Execute(string functionName)
    {
        if (!TryExecute(functionName, out var value))
            throw new NotImplementedFunctionException(functionName);
        return value;
    }

    public bool TryExecute(string functionName, out Type type)
    {
        var name = functionName.ToKebabCase();
        name = name.Replace("date-time", "dateTime");
        if (Mapping.TryGetValue(name, out var value))
        {
            type = value;
            return true;
        }

        type = null!;
        return false;
    }

    protected abstract IDictionary<string, Type> Initialize();
}
