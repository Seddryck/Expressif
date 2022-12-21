using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{
    /// <summary>
    /// Returns the argument string without white-space characters
    /// </summary>
    class WithoutWhitespaces : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => RemoveWhitespaces(value);

        private string RemoveWhitespaces(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                int len = value.Length;
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < len; i++)
                    sb.Append(value[i], char.IsWhiteSpace(value[i]) ? 0 : 1);

                return (sb.ToString());
            }
            else
            {
                return value;
            }
        }
    }
}
