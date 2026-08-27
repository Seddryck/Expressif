---
layout: docs
title: Evaluate an expression
parent: Command-line interface
nav_order: 20
description: Evaluate a closed expression, one explicit input, or one complete data source.
---

`evaluate` always evaluates an expression once and writes one formatted value.

What changes is the value supplied to that evaluation.

| Mode | How to select it | Input to the expression |
|:--|:--|:--|
| Closed | Do not provide `--input` or `--source`. | No incoming value. |
| Explicit value | Provide `--input` or `-i`. | One parsed CLI value. |
| Complete source | Provide `--source` or `-s`. | One array containing all source rows. |

## Evaluate a closed expression

A closed expression contains everything needed for its evaluation.

```bash
expressif evaluate "5 | add(3)"
```

The command writes only the result:

```text
8
```

If the expression requires an incoming value, supply one with `--input` or use `run` for several values.

## Evaluate one input value

```bash
expressif evaluate 'absolute | add(5)' --input -12
```

```text
17
```

The value passed to `--input` uses Expressif literal syntax. Numbers, booleans, null, arrays, records, dates, temporal values, and text are supported. A simple unquoted scalar that is not another valid literal is treated as text when this is unambiguous.

Quote structured or punctuation-rich values for both Expressif and the host shell:

```bash
expressif evaluate 'count' --input '{1, 2, 3}'
```

`--input` can be supplied only once. To process several independent inputs, use [`run`](run-input-data.md).

## Evaluate a complete source

With `--source`, all rows are materialized into one array before evaluation.

```bash
expressif evaluate 'sum' --source numeric.csv --scalar
```

```mermaid
flowchart LR
    A[Source rows] --> B[One array]
    B --> C[evaluate once]
    C --> D[One result]
```

`--source` and `--input` cannot be combined. `--scalar` requires `--source` and a tabular source with exactly one column.

CSV files are read as tabular data. Other file extensions are interpreted as strict UTF-8 Expressif source expressions; the source expression must be closed and return an enumerable value or `IDataReader`.

See [Run expressions over input data](run-input-data.md) for CSV profiles and for evaluating once per source row.

## Read the expression from a file

Supply the expression inline or with `--file`/`-f`, but not both.

```bash
expressif evaluate --file ./expressions/transform.expr
```

Expression files are strict UTF-8, may span several lines, and cannot be empty or contain only whitespace. Relative paths are resolved from the current working directory.

## Read the result

Successful output contains no `Result:` prefix, which makes it suitable for pipelines and command substitution. A null result is rendered as `null`.

Failures are written to standard error. The exit status distinguishes invalid expressions or inputs from failures that occur during evaluation; see [Automation and exit codes](automation.md).
