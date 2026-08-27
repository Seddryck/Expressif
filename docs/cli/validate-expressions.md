---
layout: docs
title: Validate expressions
parent: Command-line interface
nav_order: 40
description: Parse, bind, and compile expressions without evaluating them.
---

`validate` checks an expression without evaluating it.

It parses the source, resolves functions and parameters, binds the expression, and verifies that it can be compiled in the requested mode.

## Validate an open expression

Open validation is the default. It allows the expression to consume an incoming value.

```bash
expressif validate 'upper'
expressif validate 'upper' --open
```

Use open validation for transformations that will later receive input through `evaluate --input` or `run`.

## Validate a closed expression

A closed expression must be self-contained and evaluable without an incoming value.

```bash
expressif validate '5 | add(3)' --closed
```

Closed validation is useful for constants, source expressions, and configuration-time computations.

`--open` and `--closed` cannot be combined.

## Read an expression file

Supply the expression inline or with `--file`/`-f`, but not both.

```bash
expressif validate --file ./expressions/transform.expr --closed
```

Files are read as strict UTF-8. Missing, directory, undecodable, empty, and invalid files produce diagnostics on standard error.

## Understand validation results

On success, the command returns exit code `0` and writes:

```text
Expression is valid.
```

Validation catches:

- syntax errors;
- binding failures;
- unknown or unsupported functions;
- missing or unexpected function parameters;
- an input-dependent expression validated as closed.

It does not execute the expression, so it cannot detect failures that depend on a runtime value.

## Read diagnostics

Syntax diagnostics identify the source position:

```text
Syntax error [EXPR1001] at line <line>, column <column>:
  <source line>
  ^^^
Unexpected '<token>'.
```

Binding diagnostics use `EXPR2001` and explain what could not be resolved. Several syntax errors are separated by a blank line.

## Use validation in continuous integration

Validation is a fast compile gate for checked-in expressions:

```bash
expressif validate --file ./expressions/transform.expr --closed
case $? in
  0) echo 'valid' ;;
  2) echo 'invalid expression or invocation' >&2; exit 2 ;;
  1) echo 'unexpected CLI failure' >&2; exit 1 ;;
  *) echo 'unexpected status' >&2; exit 1 ;;
esac
```

Use [inspection](inspect-expressions.md) when you need to understand how the source was parsed or bound. See [Automation and exit codes](automation.md) for the complete stream and status contract.
