using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Library.Operators;

public class BinaryOperatorFactory : BaseExpressionFactory
{
    public BinaryOperatorFactory()
        : this(new OperatorRegistry<IBinaryOperator>(new AssemblyTypeSource([typeof(BinaryOperatorFactory).Assembly]))) { }

    public BinaryOperatorFactory(IImplementationRegistry registry)
        : base(registry, new AssemblyTypeSource([typeof(BinaryOperatorFactory).Assembly]))
    { }

    public IBinaryOperator Instantiate(string operatorName, IPredicate left, IPredicate right)
    {
        var type = Registry.Resolve(operatorName);
        var ctor = GetMatchingConstructor(type, 2);
        return (IBinaryOperator)ctor.Invoke(new[] { left, right });
    }
}
