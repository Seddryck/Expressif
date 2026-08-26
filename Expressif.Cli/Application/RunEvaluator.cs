using System.Collections;
using System.Reflection;
using Expressif.Cli.Commands;

namespace Expressif.Cli.Application;

internal static class RunEvaluator
{
    public static IEnumerable<object?> Evaluate(IExpression expression, Context context, IEnumerable inputs)
    {
        var enumerator = inputs.GetEnumerator();
        var index = 0;
        try
        {
            while (true)
            {
                object? input;
                try
                {
                    if (!enumerator.MoveNext())
                        yield break;
                    input = enumerator.Current;
                }
                catch (Exception exception) when (exception is not OutOfMemoryException and not FormatException)
                {
                    throw new InvalidOperationException(CommandErrorFormatter.FormatRuntimeError(exception, index + 1), exception);
                }

                object? result;
                try
                {
                    context.CurrentObject.Set(input);
                    result = expression.Evaluate(input);
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    var root = exception is TargetInvocationException invocation ? invocation.InnerException ?? exception : exception;
                    throw new InvalidOperationException(CommandErrorFormatter.FormatEvaluationError(root, index + 1), root);
                }
                yield return result;
                index++;
            }
        }
        finally { (enumerator as IDisposable)?.Dispose(); }
    }
}
