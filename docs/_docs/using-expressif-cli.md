---
title: Using the Expressif CLI
sub-title: Evaluate and validate Expressif expressions from a terminal or automation script
tags:
  - expressif
  - cli
  - command-line
  - evaluation
  - validation
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

Use `evaluate` to execute an Expressif expression.

```text
expressif evaluate <expression> [--input <value>]
```

The expression is a required positional argument. Enclose it in quotes when it contains spaces or characters interpreted by your shell.

### Evaluate a standalone expression

```console
expressif evaluate "absolute(-12)"
```

The result is written directly to standard output:

```text
12
```

### Pass an input value

Use `--input`, or its short form `-i`, to pass a value to the expression:

```console
expressif evaluate "absolute | add(5)" --input -12
```

```text
17
```

The short form is equivalent:

```console
expressif evaluate "absolute | add(5)" -i -12
```

The input is passed to the expression as text. The expression is responsible for interpreting or converting it as needed.

### Null results

When an expression returns no value, the CLI writes:

```text
null
```

### Evaluation failures

If the expression is valid but fails during evaluation, the error message is written to standard error and the command returns exit code `3`.

## Validating an expression

Use `validate` to check whether an expression can be parsed and constructed without evaluating it.

```text
expressif validate <expression>
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
