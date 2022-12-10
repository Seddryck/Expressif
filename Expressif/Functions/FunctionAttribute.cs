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
        public bool IsCompactName { get; }
        public string[] Aliases { get; }

        public FunctionAttribute() 
            : this(true) { }
        public FunctionAttribute(bool isCompactName) 
            : this(isCompactName, Array.Empty<string>()) { }
        public FunctionAttribute(bool isCompactName, string[] aliases) 
            => (IsCompactName, Aliases) = (isCompactName, aliases);
    }
}
