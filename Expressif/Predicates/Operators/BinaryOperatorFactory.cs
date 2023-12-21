using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif.Predicates.Operators;
public class BinaryOperatorFactory : BaseExpressionFactory
{
    public BinaryOperatorFactory()
        : this(new OperatorTypeMapper<IBinaryOperator>()) { }

    public BinaryOperatorFactory(BaseTypeMapper typeMapper)
        : base(typeMapper)
    { }

    public IBinaryOperator Instantiate(string operatorName, IPredicate left, IPredicate right)
    {
        var type = TypeMapper.Execute(operatorName);
        var ctor = GetMatchingConstructor(type, 2);
        return (IBinaryOperator)ctor.Invoke(new[] { left, right });
    }
}
