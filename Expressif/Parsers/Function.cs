using Sprache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Parsers
{
    public class Function
    {
        public static readonly Parser<Function> Parser =
            from functionName in Grammar.FunctionName
            from parameters in Parsers.Parameters.Parser.Optional()
            select new Function(functionName, parameters.GetOrElse(Array.Empty<IParameter>()));

        public string Name { get; }
        public IParameter[] Parameters { get; }

        public Function(string name, IParameter[] parameters)
            => (Name, Parameters) = (name, parameters);
    }
}
