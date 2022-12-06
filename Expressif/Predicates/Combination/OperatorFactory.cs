using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    internal class OperatorFactory : BaseExpressionFactory
    {
        public ICombinationOperator? Instantiate(string operatorName, Predication rightMember)
        {
            var @operator = GetFunctionType<ICombinationOperator>($"{operatorName.ToLowerInvariant()}-operator");
            return null;
        }

    }
}