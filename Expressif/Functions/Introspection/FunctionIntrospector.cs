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
                        , function.Attribute.Prefix != null && string.IsNullOrEmpty(function.Attribute.Prefix)
                            ? function.Attribute.Aliases
                            : function.Attribute.Aliases.AsQueryable()
                                .Prepend(string.IsNullOrEmpty(function.Attribute.Prefix) 
                                    ? $"{function.Type.Namespace!.Split('.').Last().ToKebabCase()}-to-{function.Type.Name.ToKebabCase()}"
                                    : $"{function.Attribute.Prefix}-to-{function.Type.Name.ToKebabCase()}"
                                ).Where(x => !string.IsNullOrEmpty(x)).ToArray()
                        , function.Type.Namespace!.ToToken('.').Last()
                        , function.Type
                        , function.Type.GetSummary()
                        , BuildParameters(function.Type.GetInfoConstructors()).ToArray()
                    );
            }
        }

        protected IEnumerable<ParameterInfo> BuildParameters(CtorInfo[] ctorInfos)
            => ctorInfos.SelectMany(x => x.Parameters)
                        .Select(y => new ParameterInfo(
                                    y.Name
                                    , !ctorInfos.All(c => c.Parameters.Any(p => p.Name == y.Name))
                                    , ctorInfos.First(c => c.Parameters.Any(p => p.Name == y.Name))
                                                    .Parameters
                                                    .Single(p => p.Name == y.Name)
                                                    .Summary
                            )
                        ).Distinct();
    }
}
