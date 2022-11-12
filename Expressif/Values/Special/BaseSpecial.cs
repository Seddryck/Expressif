using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Values.Special
{
    public abstract class BaseSpecial
    {
        public string Keyword { get; }

        public BaseSpecial(string keyword)
            => Keyword = keyword;

        protected virtual bool AdvancedMatch(string value)
            => value.ToLower().Trim().Equals(Keyword);

        public static bool operator ==(BaseSpecial left, object? right)
            => left.Equals(right);
        
        public static bool operator !=(BaseSpecial left, object? right)
            => !left.Equals(right);

        public override bool Equals(object? value)
            => false;

        public override int GetHashCode() => base.GetHashCode();
    }
}
