using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Predicates.Operators;
public class UnaryOperatorFactory<P> : BaseExpressionFactory where P: IPredicate
{
    public UnaryOperatorFactory()
        : this(new OperatorTypeMapper<IUnaryOperator>()) { }

    public UnaryOperatorFactory(BaseTypeMapper typeSetter)
        : base(typeSetter)
    { }

    public IUnaryOperator Instantiate(string operatorName, IPredicate predicate)
    {
        var type = TypeMapper.Execute(operatorName);
        var ctor = GetMatchingConstructor(type, 1);
        return (IUnaryOperator)ctor.Invoke(new[] { predicate });
    }

    protected internal override ConstructorInfo GetMatchingConstructor(Type type, int paramCount)
    {
        var genericType = type.MakeGenericType(typeof(P));
        return base.GetMatchingConstructor(genericType, paramCount);
    }
}
