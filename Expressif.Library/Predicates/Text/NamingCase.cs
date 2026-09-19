using Expressif.Functions;

namespace Expressif.Predicates.Text;

/// <summary>
/// Returns `true` when the complete input is a kebab-case identifier beginning with a letter. Words contain lowercase or uncased Unicode letters, combining marks, and numbers, separated by single hyphens. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.
/// </summary>
[Predicate(appendIs: false, prefix: "")]
public class IsKebabCase : BaseTextPredicateWithoutReference, IFunction<string?, bool>
{
    public bool Evaluate(string? value) => value is not null && EvaluateBaseText(value);
    protected override bool EvaluateText(string value) => NamingCaseValidator.IsSeparated(value, '-');
}

/// <summary>
/// Returns `true` when the complete input is a snake-case identifier beginning with a letter. Words contain lowercase or uncased Unicode letters, combining marks, and numbers, separated by single underscores. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.
/// </summary>
[Predicate(appendIs: false, prefix: "")]
public class IsSnakeCase : BaseTextPredicateWithoutReference, IFunction<string?, bool>
{
    public bool Evaluate(string? value) => value is not null && EvaluateBaseText(value);
    protected override bool EvaluateText(string value) => NamingCaseValidator.IsSeparated(value, '_');
}

/// <summary>
/// Returns `true` when the complete input is a camel-case identifier beginning with a letter. The initial letter is lowercase; subsequent Unicode letters, combining marks, and numbers may include uppercase acronym runs, as in `httpServerURL`. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.
/// </summary>
[Predicate(appendIs: false, prefix: "")]
public class IsCamelCase : BaseTextPredicateWithoutReference, IFunction<string?, bool>
{
    public bool Evaluate(string? value) => value is not null && EvaluateBaseText(value);
    protected override bool EvaluateText(string value) => NamingCaseValidator.IsCased(value, false);
}

/// <summary>
/// Returns `true` when the complete input is a pascal-case identifier beginning with a letter. The initial letter is uppercase or titlecase; subsequent Unicode letters, combining marks, and numbers may include uppercase acronym runs, as in `HTTPServer`. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.
/// </summary>
[Predicate(appendIs: false, prefix: "")]
public class IsPascalCase : BaseTextPredicateWithoutReference, IFunction<string?, bool>
{
    public bool Evaluate(string? value) => value is not null && EvaluateBaseText(value);
    protected override bool EvaluateText(string value) => NamingCaseValidator.IsCased(value, true);
}
