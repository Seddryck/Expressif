using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Values.Special
{
    public class Whitespace : BaseSpecial
    {
        private const string WHITESPACE_KEYWORD_DEFAULT = "(blank)";

        public Whitespace()
            : this(WHITESPACE_KEYWORD_DEFAULT) { }

        public Whitespace(string keyword)
            : base(keyword) { }

        public override bool Equals(object? value)
            => value switch
            {
                string v => string.IsNullOrWhiteSpace(v) || AdvancedMatch(v),
                _ => false,
            };

        public override int GetHashCode() => WHITESPACE_KEYWORD_DEFAULT.GetHashCode();
    }
}
