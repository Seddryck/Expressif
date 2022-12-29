using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{

    /// <summary>
    /// Returns a dateTime value matching the argument value parsed by the long format in the culture specified in parameter.
    /// </summary>
    [Function(prefix: "")]
    public class TextToDateTime : BaseTextFunction
    {
        public Func<string> Format { get; }
        public Func<string> Culture { get; }

        /// <param name="format">A string representing the required format.</param>
        public TextToDateTime(Func<string> format)
            => (Format, Culture) = (format, () => string.Empty);

        /// <param name="format">A string representing the required format.</param>
        /// <param name="culture">A string representing a pre-defined culture.</param>
        public TextToDateTime(Func<string> format, Func<string> culture)
            => (Format, Culture) = (format, culture);

        protected override object EvaluateString(string value)
        {
            var info = (string.IsNullOrEmpty(Culture.Invoke()) ? CultureInfo.InvariantCulture : new CultureInfo(Culture.Invoke()!)).DateTimeFormat;

            if (DateTime.TryParseExact(value, Format.Invoke(), info, DateTimeStyles.RoundtripKind, out var dateTime))
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

            throw new ArgumentException($"Impossible to transform the value '{value}' into a date using the format '{Format}'");
        }
    }

}
