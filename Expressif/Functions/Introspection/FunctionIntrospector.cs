using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Introspection
{
    public class FunctionIntrospector : BaseIntrospector
    {
        public FunctionIntrospector()
            : this(new AssemblyTypesProbe()) { }
        public FunctionIntrospector(Assembly[] assemblies)
            : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }
        public FunctionIntrospector(ITypesProbe probe)
            : base(probe) { }

        public IEnumerable<FunctionInfo> Locate()
            => Locate<FunctionAttribute>();

        protected IEnumerable<FunctionInfo> Locate<T>() where T : FunctionAttribute
        {
            var functions = LocateAttribute<FunctionAttribute>();

            foreach (var function in functions)
            {
                yield return new FunctionInfo(
                        function.Type.Name.ToKebabCase()
                        , function.Attribute.Aliases.AsQueryable()
                            .Prepend(function.Attribute.IsCompactName 
                                ? function.Type.Name.ToKebabCase().ToToken().Skip(2).ToKebabCase()
                                : string.Empty
                            ).Where(x => !string.IsNullOrEmpty(x)).ToArray()
                        , function.Type.Namespace!.ToToken('.').Last()
                        , function.Type
                    );
            }
        }

    }
}
