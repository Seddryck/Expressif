---
layout: docs
title: Run expressions over input data
parent: Command-line interface
nav_order: 30
description: Apply one open expression to repeated values, a batch, a CSV file, or an enumerable source expression.
---

`run` parses and binds an expression as open once, then evaluates it for every generated input row.

Each result is written on its own line. Processing stops at the first source or evaluation failure.

```mermaid
flowchart LR
    A[Input rows] --> B[Parse + bind expression once]
    B --> C[Evaluate for each row]
    C --> D[One output line per row]
```

## Choose an input mode

| Mode | Row behavior | Compatibility |
|:--|:--|:--|
| `--input` / `-i` | Each occurrence supplies exactly one row. | Can be combined with `--batch`. |
| `--batch` | Each direct element of one enumerable becomes a row. | Can be combined with `--input`. |
| `--source` / `-s` | Rows are read from a CSV file or source expression. | Cannot be combined with `--input` or `--batch`. |

At least one input mode is required.

## Supply repeated inputs

```bash
expressif run 'add(1)' --input 1 --input '{2, 3}' --input 4
```

This creates three rows: the number `1`, the array `{2, 3}`, and the number `4`. An array supplied with `--input` remains one row; it is not flattened.

## Expand an enumerable batch

```bash
expressif run 'add(1)' --batch '{1, 2, 3}'
```

```text
2
3
4
```

`--batch` can appear only once. Its value must be enumerable and cannot be text or a record. Only the direct elements become rows, so nested arrays remain intact.

When repeated inputs and a batch are combined, `--input` rows are evaluated before the batch elements.

## Run over a CSV file

```bash
expressif run '.name | upper' --source people.csv
```

The `.csv` extension is matched without regard to case. By default, the first row supplies field names and every following row becomes a record.

Header names must be non-empty and unique without regard to case. Every data row must have the expected number of fields. Empty text remains empty text, while database `DBNull` values become Expressif null values.

Set `header=false` for a headerless file:

```bash
expressif run '.column1 | upper' --source people.csv \
  --source-option 'header=false'
```

Generated field names are `column1`, `column2`, and so on.

### Project a single column as a scalar

By default, a tabular row is a record. `--scalar` projects the only column directly:

```bash
expressif run 'absolute | add(1)' --source values.csv --scalar
```

The source must expose exactly one column.

### Configure the CSV profile

Repeat `--source-option` with `name=value` assignments. Option names are case-sensitive and values use Expressif literal syntax.

```bash
expressif run '.name | upper' --source people.csv \
  --source-option 'delimiter=";"' \
  --source-option 'header=true'
```

Common options include:

| Option | Value | Purpose |
|:--|:--|:--|
| `delimiter` | one character | Set the field separator. |
| `quote-char` | one character or null | Set or disable the quote character. |
| `escape-char` | one character or null | Set an explicit escape character. |
| `header` | boolean | Indicate whether header rows exist. |
| `header-rows` | non-empty array of one-based integers | Select physical rows used to construct headers. |
| `comment-char` | one character or null | Mark comment rows. |
| `comment-rows` | non-empty array of one-based integers | Ignore selected physical rows. |
| `skip-initial-space` | boolean | Control whitespace after delimiters. |
| `array-delimiter` | one character or null | Split embedded array values. |

Additional profile options are visible with `expressif run --help`. Unknown options, malformed assignments, wrong value types, and incompatible combinations are rejected before evaluation.

## Run over an Expressif source

A non-CSV source file is treated as an Expressif expression:

```bash
expressif run 'absolute | add(1)' --source values.expr
```

The CLI reads the file as strict UTF-8, parses and binds it as a closed expression, evaluates it once, and uses each direct element of the resulting enumerable as a row. A null, text, record, or other scalar result is not a valid row source.

CSV source options and `--scalar` do not apply to non-tabular enumerable sources.
