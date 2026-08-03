---
title: Using the Expressif CLI
sub-title: Evaluate and validate Expressif expressions from a terminal or automation script
tags: [CLI, evaluation, validation]
---

The Expressif CLI provides a command-line interface for evaluating and validating Expressif expressions. It can be used interactively from a terminal or integrated into scripts and continuous integration workflows.

## Command syntax

The general command syntax is:

```text
expressif <command> [arguments] [options]
```

The CLI provides three commands:

| Command | Purpose |
|---|---|
| `evaluate` | Evaluate an Expressif expression |
| `validate` | Validate an expression without evaluating it |
| `version` | Display the installed CLI and library versions |

Use the built-in help to discover the available commands:

```console
expressif --help
```

Help is also available for each command:

```console
expressif evaluate --help
expressif validate --help
expressif version --help
```

## Evaluating an expression

Use `evaluate` to execute an expression. The expression can be supplied inline or loaded from a file.

### Supplying the expression

Provide the expression inline as a positional argument:

```console
expressif evaluate "absolute | add(5)"
```

Or load it from a UTF-8 file with `--file` (alias: `-f`):

```console
expressif evaluate --file ./expressions/transform.expr
```

The expression must be provided through exactly one source:

- inline argument; or
- `--file` / `-f`.

Providing both sources, or neither source, returns an error.

### Input-based evaluation

Use `--input` (alias: `-i`) when the expression should be evaluated against an explicit input value.

```console
expressif evaluate "absolute | add(5)" --input -12
```

The result is written directly to standard output:

```text
17
```

The short form is equivalent:

```console
expressif evaluate "absolute | add(5)" -i -12
```

### ClosedExpression evaluation (no input)

When `--input` is not provided, the expression is evaluated as a `ClosedExpression` and executed exactly once.

```console
expressif evaluate "5 | add(3)"
```

```text
8
```

This path is intended for expressions fully defined by literals, variables, context parameters, and functions.

If the expression requires an input (for example an open expression like `upper`), evaluation fails with an explicit message instructing you to use `--input`.

### Evaluating from a file

Expression files are read as UTF-8 and may contain multiline expressions.

```text
5
| add(3)
| multiply(2)
```

```console
expressif evaluate --file calculation.expr
```

```text
16
```

Relative paths are resolved from the current working directory. Absolute paths are also supported.

### Null results

When an expression returns no value, the CLI writes:

```text
null
```

### Evaluation failures

If the expression is valid but fails during evaluation, the error message is written to standard error and the command returns exit code `3`.

### Expression file errors

Expression-file loading returns clear diagnostics when:

- the file does not exist;
- the path points to a directory;
- the file cannot be accessed;
- the file is empty or whitespace only;
- the file content is not valid UTF-8;
- the loaded expression is invalid.

## Validating an expression

Use `validate` to check whether an expression can be parsed and constructed without evaluating it.

As with `evaluate`, the expression can be provided inline or loaded from a file.

By default, `validate` checks the expression as an open expression.

You can provide `--open` explicitly, but this is the default behavior.

Use `--closed` to validate the expression strictly as a closed expression.

`--open` and `--closed` are mutually exclusive.

```text
expressif validate <expression>
```

You can also load a UTF-8 expression file:

```console
expressif validate --file ./expressions/transform.expr
```

The short form is equivalent:

```console
expressif validate -f ./expressions/transform.expr
```

For `validate`, the expression must be supplied through exactly one source:

- inline argument; or
- `--file` / `-f`.

Examples:

```console
expressif validate "upper"
```

```console
expressif validate "upper" --open
```

```console
expressif validate "5 | add(3)" --closed
```

For example:

```console
expressif validate "absolute | add(5)"
```

A valid expression produces:

```text
Expression is valid.
```

An invalid expression writes a diagnostic message to standard error and returns exit code `2`.

Validation detects issues such as:

- invalid expression syntax;
- unknown or unsupported functions;
- missing or unexpected function parameters.

Validation does not execute the expression and does not require an input value.

## Displaying the version

Use `version` to display both the CLI version and the version of the Expressif library used by it:

```console
expressif version
```

The output follows this format:

```text
Expressif CLI <version>
Expressif <version>
```

Including this output in issue reports or CI logs helps identify the exact CLI and library versions involved.

## Quoting expressions

Expressions should generally be enclosed in quotes to prevent the shell from interpreting spaces, parentheses, pipes, or other special characters.

### PowerShell

Use double or single quotes:

```powershell
expressif evaluate 'absolute | add(5)' --input -12
```

Single quotes are often the easiest choice when the expression itself contains double-quoted text.

### Windows Command Prompt

Use double quotes:

```cmd
expressif evaluate "absolute | add(5)" --input -12
```

### Bash

Use single quotes where possible:

```bash
expressif evaluate 'absolute | add(5)' --input -12
```

Quoting rules belong to the shell, not to Expressif. When an expression contains quotes of its own, escape them according to the rules of the shell being used.

## Output and diagnostics

The CLI follows standard command-line conventions:

| Stream | Content |
|---|---|
| Standard output | Evaluation results, validation confirmation, and version information |
| Standard error | Invalid expressions, evaluation failures, and unexpected errors |

Successful evaluation output contains only the resulting value. The CLI does not prefix it with labels such as `Result:`. This makes the output easier to capture or pipe into another command.

## Exit codes

Expressif uses exit codes to indicate the outcome of a command:

| Exit code | Meaning |
|---:|---|
| `0` | The command completed successfully |
| `1` | An unexpected internal error occurred |
| `2` | The expression or command input is invalid |
| `3` | The expression was valid but its evaluation failed |

### PowerShell

```powershell
expressif validate $expression

if ($LASTEXITCODE -ne 0) {
    throw "The Expressif expression is invalid."
}
```

To distinguish validation errors from unexpected failures:

```powershell
expressif validate $expression

switch ($LASTEXITCODE) {
    0 { Write-Host "Expression is valid." }
    1 { throw "Expressif encountered an unexpected internal error." }
    2 { throw "The expression is invalid." }
    default { throw "Expressif returned unexpected exit code $LASTEXITCODE." }
}
```

### Bash

```bash
if expressif validate "$expression"; then
    echo "Expression is valid."
else
    echo "Expression is invalid." >&2
    exit 1
fi
```

Evaluation output can be captured directly:

```bash
result="$(expressif evaluate 'absolute | add(5)' --input -12)"
echo "$result"
```

## Next steps

See the Expressif language reference for the available functions, predicates, accumulators, and pipeline syntax.
