using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Expressif.Values.Special
{
    public class Value : BaseSpecial
    {
        private const string VALUE_KEYWORD_DEFAULT = "(value)";

        public Value()
            : this(VALUE_KEYWORD_DEFAULT) { }

        public Value(string keyword)
            : base(keyword) { }

        public override bool Equals(object? value)
            => value switch
            {
                Null => false,
                null => false,
                DBNull => false,
                string v => AdvancedMatch(v),
                _ => true,
            };

        protected override bool AdvancedMatch(string value)
            => base.AdvancedMatch(value) || !new Null().Equals(value);

        public override int GetHashCode() => VALUE_KEYWORD_DEFAULT.GetHashCode();
    }
}
