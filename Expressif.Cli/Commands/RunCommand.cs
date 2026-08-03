using System.Collections;
using System.CommandLine;
using System.Globalization;
using System.Text;

namespace Expressif.Cli.Commands;

internal static class RunCommand
{
    private static readonly Func<string, Context, Expression> DefaultBuildExpression = static (code, context) => new Expression(code, context);
    private static readonly Func<string, object?> DefaultParseInput = static input => InputValueParser.Parse(input);
    private static readonly Func<Expression, IEnumerable, IEnumerable<object?>> DefaultRunExpression = static (expression, inputs) => EvaluateEach(expression, inputs);

    internal static Func<string, Context, Expression> BuildExpression { get; set; }
        = DefaultBuildExpression;

    internal static Func<string, object?> ParseInput { get; set; }
        = DefaultParseInput;

    internal static Func<Expression, IEnumerable, IEnumerable<object?>> RunExpression { get; set; }
        = DefaultRunExpression;

    internal static void ResetDelegates()
    {
        BuildExpression = DefaultBuildExpression;
        ParseInput = DefaultParseInput;
        RunExpression = DefaultRunExpression;
    }

    public static Command Create()
    {
        var expressionArgument = new Argument<string?>("expression")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Expression to evaluate."
        };

        var inputOption = new Option<string[]>("--input")
        {
            Description = "Input row passed to the expression. Repeat --input to add rows."
        };
        inputOption.Aliases.Add("-i");

        var batchOption = new Option<string?>("--batch")
        {
            Description = "Enumerable batch input. Each direct element is evaluated as one row."
        };

        var expressionFileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 file containing the expression to evaluate."
        };
        expressionFileOption.Aliases.Add("-f");

        var command = new Command("run", "Evaluate an Expressif expression for each element of an input sequence.");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(inputOption);
        command.Options.Add(batchOption);
        command.Options.Add(expressionFileOption);

        command.SetAction(parseResult =>
        {
            var inlineExpression = parseResult.GetValue(expressionArgument);
            var expressionFilePath = parseResult.GetValue(expressionFileOption);
            var inputRows = parseResult.GetValue(inputOption) ?? [];
            var batchInput = parseResult.GetValue(batchOption);
            var hasInputOption = parseResult.GetResult(inputOption) is not null;
            var hasBatchOption = parseResult.GetResult(batchOption) is not null;

            var batchOptionOccurrences = parseResult.Tokens.Count(token => token.Value is "--batch");
            if (batchOptionOccurrences > 1)
            {
                Console.Error.WriteLine("The --batch option can only be specified once.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!ExpressionCommandCommon.TryResolveExpressionCode(
                    inlineExpression,
                    expressionFilePath,
                    out var expressionCode,
                    out var hasExpressionFile))
            {
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!hasInputOption && !hasBatchOption)
            {
                Console.Error.WriteLine("The run command requires inputs. Provide at least one --input row or one --batch enumerable value.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            var sequenceInput = BuildInputSequence(inputRows, hasBatchOption, batchInput);

            Expression openExpression;
            try
            {
                openExpression = BuildExpression(expressionCode, new Context());
            }
            catch (Exception exception) when (exception is Sprache.ParseException
                                              or NotImplementedFunctionException
                                              or MissingOrUnexpectedParametersFunctionException)
            {
                return ExpressionCommandCommon.WriteValidationError(exception, hasExpressionFile, expressionFilePath);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unexpected error: {exception.Message}");
                return ExitCodes.UnexpectedInternalError;
            }

            try
            {
                foreach (var result in RunExpression(openExpression, sequenceInput))
                    Console.Out.WriteLine(result ?? "null");

                return ExitCodes.Success;
            }
            catch (FormatException exception)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.InvalidExpressionOrInput;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.EvaluationFailed;
            }
        });

        return command;
    }

    private static IEnumerable<object?> EvaluateEach(Expression expression, IEnumerable inputs)
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
                    throw new InvalidOperationException(
                        $"Input enumeration failed at position {index}: {exception.Message}",
                        exception);
                }

                object? result;
                try
                {
                    result = expression.Evaluate(input);
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    throw new InvalidOperationException(
                        $"Expression evaluation failed for input {FormatValue(input)}: {exception.Message}",
                        exception);
                }

                yield return result;
                index++;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private static IEnumerable<object?> BuildInputSequence(
        IEnumerable<string> inputRows,
        bool hasBatchOption,
        string? batchInput)
    {
        foreach (var input in inputRows)
        {
            object? parsedInput;
            try
            {
                parsedInput = ParseInput(input);
            }
            catch (FormatException exception)
            {
                throw new FormatException($"Invalid input syntax for --input '{input}': {exception.Message}", exception);
            }

            // Each --input occurrence defines one row as-is, including array values.
            yield return parsedInput;
        }

        if (!hasBatchOption)
            yield break;

        object? parsedBatchInput;
        try
        {
            parsedBatchInput = ParseInput(batchInput ?? string.Empty);
        }
        catch (FormatException exception)
        {
            throw new FormatException($"Invalid input syntax for --batch '{batchInput}': {exception.Message}", exception);
        }

        if (parsedBatchInput is not IEnumerable enumerable || parsedBatchInput is string)
            throw new FormatException("The --batch option requires an enumerable value.");

        var enumerator = enumerable.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
            return "null";

        if (value is string text)
            return text;

        if (value is IEnumerable enumerable)
        {
            var values = new List<string>();
            foreach (var item in enumerable)
                values.Add(FormatValue(item));

            return $"{{{string.Join(", ", values)}}}";
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? "null";
    }

    internal static class InputValueParser
    {
        public static object? Parse(string text)
        {
            var parser = new Parser(text);
            var value = parser.ParseValue();
            parser.SkipWhitespace();
            if (!parser.IsAtEnd)
                throw new FormatException($"Unexpected token '{parser.Current}' at position {parser.Position + 1}.");

            return value;
        }

        private sealed class Parser
        {
            private readonly string text;
            private int position;

            public Parser(string text)
                => this.text = text ?? string.Empty;

            public bool IsAtEnd => position >= text.Length;
            public int Position => position;
            public char Current => text[position];

            public void SkipWhitespace()
            {
                while (!IsAtEnd && char.IsWhiteSpace(Current))
                    position++;
            }

            public object? ParseValue()
            {
                SkipWhitespace();
                if (IsAtEnd)
                    return string.Empty;

                if (Current == '{')
                    return ParseArray();

                if (Current == '"' || Current == '`')
                    return ParseQuoted();

                return ParseScalar();
            }

            private object?[] ParseArray()
            {
                position++; // '{'
                SkipWhitespace();

                var values = new List<object?>();
                if (!IsAtEnd && Current == '}')
                {
                    position++;
                    return values.ToArray();
                }

                while (true)
                {
                    values.Add(ParseValue());
                    SkipWhitespace();

                    if (IsAtEnd)
                        throw new FormatException("Unterminated array literal.");

                    if (Current == ',')
                    {
                        position++;
                        continue;
                    }

                    if (Current == '}')
                    {
                        position++;
                        break;
                    }

                    throw new FormatException($"Unexpected token '{Current}' at position {position + 1}. Expected ',' or '}}'.");
                }

                return values.ToArray();
            }

            private string ParseQuoted()
            {
                var quote = Current;
                position++;

                var builder = new StringBuilder();
                while (!IsAtEnd && Current != quote)
                {
                    builder.Append(Current);
                    position++;
                }

                if (IsAtEnd)
                    throw new FormatException("Unterminated quoted input value.");

                position++; // closing quote
                return builder.ToString();
            }

            private object ParseScalar()
            {
                var start = position;
                while (!IsAtEnd && Current != ',' && Current != '}')
                    position++;

                var token = text[start..position].Trim();
                if (token.Length == 0)
                    throw new FormatException($"Expected a value at position {start + 1}.");

                if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
                    return integer;

                if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeric))
                    return numeric;

                if (bool.TryParse(token, out var boolean))
                    return boolean;

                if (string.Equals(token, "null", StringComparison.OrdinalIgnoreCase))
                    return null!;

                return token;
            }
        }
    }
}
