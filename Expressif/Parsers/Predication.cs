using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Parsers
{
    public class Predication
    {
        public Function Member { get; }

        public Predication(Function member)
            => (Member) = (member);

        public static readonly Parser<Predication> Parser =
            from predicate in Function.Parser
            select new Predication(predicate);
    }
}
