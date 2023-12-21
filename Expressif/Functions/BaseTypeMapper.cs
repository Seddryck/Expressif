using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions;

public abstract class BaseTypeMapper
{
    private IDictionary<string, Type>? _mapping;
    protected IDictionary<string, Type> Mapping { get => _mapping ??= Initialize(); }

    public Type Execute(string functionName)
    {
        var name = functionName.ToKebabCase();
        if (!Mapping.TryGetValue(name, out var value))
            throw new NotImplementedFunctionException(functionName);
        return value;
    }

    protected abstract IDictionary<string, Type> Initialize();
}
