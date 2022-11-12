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
    }
}
