using Expressif.Library.Text;

namespace Expressif.Library.Text.Normalization;

/// <summary>
/// Returns the argument value without all leading or trailing white-space characters.
/// </summary>
[Scope("text/normalization")]
public class Trim : BaseTextFunction
{
    protected override object EvaluateBlank() => Expressif.Values.Special.Empty.Keyword;
    protected override object EvaluateString(string value) => value.Trim();
}
