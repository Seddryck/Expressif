using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{

    public abstract class BasePaddingFunction : BaseTextLength
    {
        public Func<char> Character { get; }

        public BasePaddingFunction(Func<int> length, Func<char> character)
            : base(length)
            => Character = character;

        protected override object EvaluateEmpty() => new string(Character.Invoke(), Length.Invoke());
        protected override object EvaluateNull() => new string(Character.Invoke(), Length.Invoke());

    }

    /// <summary>
    /// Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. 
    /// </summary>
    public class PadRight : BasePaddingFunction
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the minimal length of the string returned</param>
        /// <param name="character">The padding character</param>
        public PadRight(Func<int> length, Func<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Invoke() ? value : value.PadRight(Length.Invoke(), Character.Invoke());
    }

    /// <summary>
    /// Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. 
    /// </summary>
    public class PadLeft : BasePaddingFunction
    {
        /// <param name="length">An integer value between 0 and +Infinity, defining the minimal length of the string returned</param>
        /// <param name="character">The padding character</param>
        public PadLeft(Func<int> length, Func<char> character)
            : base(length, character) { }

        protected override object EvaluateString(string value)
            => value.Length >= Length.Invoke() ? value : value.PadLeft(Length.Invoke(), Character.Invoke());
    }

}
