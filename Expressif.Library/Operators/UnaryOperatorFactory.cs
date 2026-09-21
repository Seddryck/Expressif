using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Library.Operators;

public class UnaryOperatorFactory : BaseExpressionFactory
{
    public UnaryOperatorFactory()
        : this(new OperatorRegistry<IUnaryOperator>(new AssemblyTypeSource([typeof(UnaryOperatorFactory).Assembly]))) { }

    public UnaryOperatorFactory(IImplementationRegistry registry)
        : base(registry, new AssemblyTypeSource([typeof(UnaryOperatorFactory).Assembly]))
    { }

    public IUnaryOperator Instantiate(string operatorName, IPredicate predicate)
    {
        var type = Registry.Resolve(operatorName);
        var ctor = GetMatchingConstructor(type, 1);
        return (IUnaryOperator)ctor.Invoke(new[] { predicate });
    }
}
