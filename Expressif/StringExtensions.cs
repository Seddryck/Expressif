using Expressif.Functions.Temporal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Expressif
{
    static class StringExtensions
    {
        public static string ToPascalCase(this string value)
            => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Trim().ToLowerInvariant().Replace("-", " "))
                .Replace(" ", "")
                .Replace("Datetime", "DateTime")
                .Replace("Timespan", "TimeSpan");

        public static string ToKebabCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            value = value
                .Replace(" ", "")
                .Replace("TimeSpan", "Timespan")
                .Replace("DateTime", "Datetime");

            var builder = new StringBuilder();
            builder.Append(char.ToLower(value.First()));

            char previous = value.First();
            foreach (var c in value.Skip(1))
            {
                if (char.IsUpper(c) && previous!='-' && !char.IsUpper(previous))
                    builder.Append('-');
                builder.Append(char.ToLower(c));
                previous = c;
            }

            return builder.ToString();
        }

        public static string ToKebabCase(this IEnumerable<string> array)
            => string.Join('-', array);

        public static string[] ToToken(this string value, char separator = '-')
         => string.IsNullOrEmpty(value) 
                ? Array.Empty<string>() 
                : value.Split(separator);


    }
}
