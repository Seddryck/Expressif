---
layout: docs
title: Inspect parsing and binding
parent: Command-line interface
nav_order: 50
description: Inspect the syntax tree produced by parsing and the semantic tree produced by binding.
---

The `parse` and `bind` commands expose two different stages of expression processing.

| Command | Stage | Question it answers |
|:--|:--|:--|
| `parse` | Syntax | How did the tokens and grammar form the syntax tree? |
| `bind` | Syntax and semantics | Which functions, parameters, fields, and expression form were resolved? |

```mermaid
flowchart LR
    A[Expression text] --> B[parse]
    B --> C[Syntax tree]
    C --> D[bind]
    D --> E[Bound expression tree]
```

## Inspect syntax

```bash
expressif parse '5 | add(3)'
```

The parse document describes each syntax node with:

- `Kind`, the syntax-node kind;
- `Text`, the exact source text represented by the node;
- `Span.Start`, its zero-based character offset;
- `Span.Length`, its number of source characters;
- `Children`, its ordered child nodes.

Use `parse` when a diagnostic position, grammar interpretation, or shell-quoting result is surprising. The exact `Text` and `Span` values reveal what Expressif received.

## Inspect binding

```bash
expressif bind '5 | add(3)'
```

Binding resolves the meaning of the parsed expression. Its output distinguishes open and closed roots and exposes resolved functions, argument positions, parameters, record fields, arrays, tuples, nested expressions, and intervals.

Use `bind` when parsing succeeds but a function, parameter, field, or open/closed classification is not what you expected.

## Choose an output format

Both commands support three formats:

```bash
expressif parse '5 | add(3)' --output tree
expressif parse '5 | add(3)' --output json
expressif bind '5 | add(3)' --output yaml
```

| Format | Best use |
|:--|:--|
| `tree` | Human inspection in a terminal or issue report. This is the default. |
| `json` | Automated tooling and assertions. |
| `yaml` | Readable structured output. |

Output-format names are matched without regard to case. Any other value is rejected.

Do not parse the human-oriented tree as a stable machine contract. Request JSON or YAML and use an appropriate parser.

## Diagnose an expression step by step

1. Run `parse` when the grammar or reported source location is unexpected.
2. Check `Text` and `Span` to detect changes introduced by shell quoting.
3. Run `bind` when syntax is valid but semantic resolution is wrong.
4. Use JSON for automated assertions and the tree view for human review.
5. Use [`validate`](validate-expressions.md) when only pass/fail and a stable exit code are needed.

`parse` and `bind` accept an inline positional expression. They do not currently support `--file`.
