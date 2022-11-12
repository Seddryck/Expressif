using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Expressif.Values.Special
{
    public class Empty : BaseSpecial
    {
        private const string EMPTY_KEYWORD_DEFAULT = "(empty)";

        public Empty()
            : this(EMPTY_KEYWORD_DEFAULT) { }

        public Empty(string keyword)
            : base(keyword) { }

        public override bool Equals(object? value)
            => value switch
            {
                Empty => true,
                string v => string.IsNullOrEmpty(v) || AdvancedMatch(v),
                _ => false,
            };

        public override int GetHashCode() => EMPTY_KEYWORD_DEFAULT.GetHashCode();
    }
}
