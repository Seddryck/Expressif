using Expressif.Functions;
using Sprache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Parsers;

public class FunctionParser
{
    public static readonly Parser<FunctionMeta> Parser =
        from functionName in Grammar.FunctionName
        from parameters in ParametersParser.Parser.Optional()
        select new FunctionMeta(functionName, parameters.GetOrElse([]));
}
