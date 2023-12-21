using System;

namespace Expressif.Values;

internal interface IScalarResolver
{
    object? Execute();
}

internal interface IScalarResolver<T> : IScalarResolver
{
    new T? Execute();
}
