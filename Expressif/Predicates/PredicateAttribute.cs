using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class PredicateAttribute : Attribute
    {
        public string[] Aliases { get; }

        public PredicateAttribute() 
            : this(Array.Empty<string>()) { }
        public PredicateAttribute(string[] aliases) 
            => (Aliases) = (aliases);
    }
}
