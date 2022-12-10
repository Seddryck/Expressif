using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class FunctionAttribute : Attribute
    {
        public string[] Aliases { get; }
        public string? Prefix { get; }

        public FunctionAttribute() 
            : this(null, Array.Empty<string>()) { }

        public FunctionAttribute(string? prefix = null, string[]? aliases=null) 
            => (Prefix, Aliases) = (prefix, aliases ?? Array.Empty<string>());
    }
}
