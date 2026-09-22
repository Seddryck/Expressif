using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Values.Special;

internal static class SpecialValue
{
    public static bool Matches(string value, string keyword)
        => value.Trim().Equals(keyword, StringComparison.OrdinalIgnoreCase);
}
