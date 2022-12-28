using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{

    public abstract class BaseTextAppend : BaseTextFunction
    {
        public Func<string> Append { get; }
        public BaseTextAppend(Func<string> append)
            => Append = append;

        protected override object EvaluateEmpty() => Append.Invoke() ?? string.Empty;
        protected override object EvaluateBlank() => Append.Invoke() ?? string.Empty;
    }

    /// <summary>
    /// Returns the argument value preceeded by the parameter value.
    /// </summary>
    public class Prefix : BaseTextAppend
    {
        /// <param name="prefix">The text to append</param>
        public Prefix(Func<string> prefix)
            : base(prefix) { }
        protected override object EvaluateString(string value) => $"{Append.Invoke()}{value}";
    }

    /// <summary>
    /// Returns the argument value followed by the parameter value.
    /// </summary>
    public class Suffix : BaseTextAppend
    {
        /// <param name="suffix">The text to append</param>
        public Suffix(Func<string> suffix)
            : base(suffix) { }
        protected override object EvaluateString(string value) => $"{value}{Append.Invoke()}";
    }

}
