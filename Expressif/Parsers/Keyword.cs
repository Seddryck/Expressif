using Expressif.Values;
using Expressif.Values.Special;
using Sprache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Parsers
{
    public class Keyword
    {
        public static readonly Parser<string> OrOperator = Parse.IgnoreCase("OR").Text();
        public static readonly Parser<string> AndOperator = Parse.IgnoreCase("AND").Text();
        public static readonly Parser<string> XorOperator = Parse.IgnoreCase("XOR").Text();
    }
}   
