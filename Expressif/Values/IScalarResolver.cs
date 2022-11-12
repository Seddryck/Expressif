using System;

namespace Expressif.Values
{
    public interface IScalarResolver
    {
        object? Execute();
    }

    public interface IScalarResolver<T> : IScalarResolver
    {
        new T? Execute();
    }
}
