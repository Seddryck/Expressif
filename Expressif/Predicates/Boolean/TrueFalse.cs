using Expressif.Predicates.Boolean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Boolean;

/// <summary>
/// Returns `true` if the argument is effectively `true` else return `false`.
/// </summary>
public class True : BaseBooleanPredicate
{
    protected override bool EvaluateBoolean(bool boolean) => boolean;
}

/// <summary>
/// Returns `true` if the argument is effectively `true` or `null` else return `false`.
/// </summary>
public class TrueOrNull : True
{
    protected override bool EvaluateNull() => true;
}

/// <summary>
/// Returns `true` if the argument is effectively `false` else return `false`.
/// </summary>
public class False : BaseBooleanPredicate
{
    protected override bool EvaluateBoolean(bool boolean) => !boolean;
}

/// <summary>
/// Returns `true` if the argument is effectively `false` or `null` else return `false`.
/// </summary>
public class FalseOrNull : False
{
    protected override bool EvaluateNull() => true;
}
