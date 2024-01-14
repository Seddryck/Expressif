using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Predicates.Operators;
public class BinaryOperatorFactory<P> : BaseExpressionFactory where P : IPredicate
{
    public BinaryOperatorFactory()
        : this(new OperatorTypeMapper<IBinaryOperator>()) { }

    public BinaryOperatorFactory(BaseTypeMapper typeMapper)
        : base(typeMapper)
    { }

    public IBinaryOperator Instantiate(string operatorName, P left, P right)
    {
        var type = TypeMapper.Execute(operatorName);
        var ctor = GetMatchingConstructor(type, 2);
        return (IBinaryOperator)ctor.Invoke([left, right]);
    }

    protected internal override ConstructorInfo GetMatchingConstructor(Type type, int paramCount)
    {
        var genericType = type.MakeGenericType(typeof(P));
        return base.GetMatchingConstructor(genericType, paramCount);
    }
}
