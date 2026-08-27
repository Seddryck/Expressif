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
| `run` | Evaluate an expression once per element of an input sequence |
| `validate` | Validate an expression without evaluating it |
| `version` | Display the installed CLI and library versions |

Use the built-in help to discover the available commands:

```console
expressif --help
```

Help is also available for each command:

```console
expressif evaluate --help
expressif run --help
expressif validate --help
expressif version --help
```

## Evaluating an expression

Use `evaluate` to execute an expression. The expression can be supplied inline or loaded from a file.

### Supplying the expression

Provide the expression inline as a positional argument:

```console
expressif evaluate "5 | add(3)"
```

Or load it from a UTF-8 file with `--file` (alias: `-f`):

```console
expressif evaluate --file ./expressions/transform.expr
```

The expression must be provided through exactly one source:

- inline argument; or
- `--file` / `-f`.

Providing both sources, or neither source, returns an error.

### Closed expression evaluation (no input)

When `--input` is not provided, the expression is validated as closed and executed exactly once.

```console
expressif evaluate "5 | add(3)"
```

```text
8
```

This path is intended for expressions fully defined by literals, variables, context parameters, and functions.

If the expression requires an input (for example an open expression like `upper`), evaluation fails with an explicit message instructing you to use `--input`.

### Input-based evaluation

Use `--input` (alias: `-i`) when the expression should be evaluated against an explicit input value.

```console
expressif evaluate "absolute | add(5)" --input -12
```
The result is written directly to standard output:

```text
12
```

The input is passed to the expression as text. The expression is responsible for interpreting or converting it as needed.

### Evaluating a complete source

Use `--source` (alias: `-s`) to load all source rows into one array and evaluate the expression once. For a one-column CSV named `numeric.csv`:

```console
expressif evaluate "sum" --source numeric.csv --scalar
```

```text
60
```

Without `--scalar`, CSV rows are records. With `--scalar`, the source must expose exactly one column and each row contributes that column's value. `evaluate` accepts the same repeatable `--source-option <name>=<value>` CSV settings documented below for `run`.

`--source` cannot be combined with `--input`. Both `--scalar` and `--source-option` require `--source`.

## Running an expression over a sequence

Use `run` to evaluate an expression repeatedly over an input sequence.

`run` accepts three complementary input options:

- `--input` (repeatable): each occurrence defines exactly one row, whether the value is scalar or enumerable;
- `--batch` (single value): the value must be enumerable, and each direct element becomes one row;
- `--source` (single file path): load rows from a source file.

How `--source` works:

- CSV files (`.csv`) use a header-and-rows pattern.
- In CSV files, the first row is treated as the header (column names).
- In CSV files, each following row is evaluated once.
- In CSV files, use column names in expressions, for example `[name]` or `[age]`.
- Non-CSV files (for example `.expr`) are evaluated as closed expressions.
- A non-CSV source expression must return an enumerable.
- Each direct element returned by a non-CSV source expression is evaluated once.

```console
expressif run "count" --input "{1, -2, 3}"
```

```text
3
```

In this example, there is one row (the array value `{1, -2, 3}`), not three rows.

The `run` command differs from `evaluate`:

- `evaluate` executes once for a single input value;
- `run` executes once per generated row.

You can pass `--input` multiple times. Each occurrence contributes one row.

```console
expressif run "add(1)" --input 1 --input "{2, 3}" --input 4
```

This command produces three rows: `1`, `{2, 3}`, and `4`.

To evaluate once per element of an enumerable value, use `--batch`.

```console
expressif run "add(1)" --batch "{1, 2, 3}"
```

```text
2
3
4
```

`--batch` requires an enumerable value.

```console
expressif run "add(1)" --input 42
```

```text
43
```

Nested enumerable elements are preserved and not flattened when using `--batch`.

```console
{% raw %}
expressif run "count" --batch "{{1, 2, 3}, {4, 5}}"
{% endraw %}
```

```text
3
2
```

### Running from a source file

Use `--source` to evaluate one row at a time from a source file.

```console
expressif run "[name] | upper" --source people.csv
```

For CSV input:

- the first row is treated as header;
- each following row is an input record;
- empty fields are represented as empty text;
- duplicate header names are rejected;
- malformed rows (wrong field count) fail with a clear error;
- processing is streamed row by row so large files can be processed efficiently.

CSV parsing can be configured by repeating `--source-option <name>=<value>`. Options apply only to the file supplied by `--source`.

Use `--scalar` for a tabular source with exactly one column when each row should be passed directly as that column's value instead of as a record.

```console
expressif run "[name] | upper" --source people.csv \
  --source-option 'delimiter=";"' \
  --source-option 'header=true' \
  --source-option 'quote-char="\""'
```

Supported CSV source options are:

| Option | Value |
| --- | --- |
| `delimiter` | Single field-separator character |
| `line-terminator` | Record-separator text |
| `quote-char` | Single character, or `null` to disable quoting |
| `double-quote` | Boolean controlling doubled-quote escaping |
| `escape-char` | Single character, or `null` to disable explicit escaping |
| `header` | Boolean indicating whether header rows are present |
| `header-rows` | Array of one-based physical row indexes, such as `{1, 2}` |
| `header-join` | Text joining values from multiple header rows |
| `header-repeat` | Boolean controlling repeated empty header cells |
| `comment-char` | Single character identifying comment rows, or `null` |
| `comment-rows` | Array of one-based physical row indexes to ignore |
| `null-sequence` | Text interpreted as null |
| `missing-cell` | Text used for a missing trailing cell |
| `skip-initial-space` | Boolean controlling whitespace after delimiters |
| `array-delimiter` | Single embedded-array separator character, or `null` |
| `array-prefix` | Single embedded-array opening character, or `null` |
| `array-suffix` | Single embedded-array closing character, or `null` |

Values use Expressif literal syntax: `true` and `false` for booleans, `null`, quoted text, and arrays such as `{1, 3}`. Unknown properties, invalid values, and incompatible profiles are rejected. CSV behavior is unchanged when no source options are supplied.

For non-CSV source files, the source expression is evaluated first and must return an enumerable sequence.

Examples:

```console
expressif run "[country] | upper" --source customers.csv
```

```console
expressif run "absolute | add(1)" --source values.expr
```

`--source` cannot be combined with `--input` or `--batch`.

Expression loading works the same way as with `evaluate`: inline argument or `--file` (`-f`).

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
