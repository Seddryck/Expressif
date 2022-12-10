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
        public bool AppendIs { get; }
        public string[] Aliases { get; }
        public string? Prefix { get; }

        public PredicateAttribute()
            : this(Array.Empty<string>()) { }

        public PredicateAttribute(bool appendIs)
            : this(appendIs, null, Array.Empty<string>()) { }

        public PredicateAttribute(string[] aliases)
            : this(true, null, aliases) { }

        public PredicateAttribute(bool appendIs = true, string? prefix = null, string[]? aliases = null)
            => (AppendIs, Prefix, Aliases) = (appendIs, prefix, aliases ?? Array.Empty<string>());
    }
}
