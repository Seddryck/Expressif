using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{
    public abstract class BaseTextCasing : BaseTextFunction
    { }

    /// <summary>
    /// Returns the argument value converted to lowercase using the casing rules of the invariant culture.
    /// </summary>
    public class Lower : BaseTextCasing
    {
        protected override object EvaluateString(string value) => value.ToLowerInvariant();
    }

    /// <summary>
    /// Returns the argument value converted to uppercase using the casing rules of the invariant culture.
    /// </summary>
    public class Upper : BaseTextCasing
    {
        protected override object EvaluateString(string value) => value.ToUpperInvariant();
    }
}
