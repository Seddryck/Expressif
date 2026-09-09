---
layout: docs
title: Analyze field scopes
parent: .NET SDK
nav_order: 8
permalink: /dotnet-sdk/semantic-analysis/
description: Find the input or enclosing scope supplying a field reference without evaluating an expression.
---

`Expressif.Semantics.SemanticAnalyzer` is part of the `Expressif` package. It identifies the value supplying each selection in canonical field-reference syntax, such as `.address.city`, `^.name`, and `^^.threshold`. It does not need input values, a schema, a context, or variable providers, and does not construct or execute runtime functions.

```csharp
using Expressif.Semantics;

const string text = ".items | map(.name | suffix(^^.suffix))";
var analysis = new SemanticAnalyzer().Analyze(text);

foreach (var reference in analysis.References)
{
    Console.WriteLine($"{reference.Kind}: {reference.Source.Kind}");
    if (reference.Source.Span is { } span)
        Console.WriteLine(text.Substring(span.Start, span.Length));
}
```

In this example, `.items` reads external input, `.name` reads an element of the value produced by `.items`, and `^^.suffix` reads the external enclosing input. The element source identifies `.items`; it does not claim to know where a `name` field was declared.

## Results and source regions

`SemanticAnalysis.References` is ordered by source offset. Each `FieldReference` contains:

- `Syntax`: the original canonical `RecordAccessSyntax` node.
- `SelectionIndex`: the zero-based selector within a chained reference.
- `Span`: the selection region, including its leading dot and, for the first selection, its root prefix.
- `Kind`: current input, current expression root, or enclosing expression root.
- `Source`: the value supplier for that selection.
- `ExpressionRoot` and `EnclosingRoot`: the evaluation roots at that location.

A `SemanticSource` distinguishes `ExternalInput`, `Expression`, `Element`, and `Unresolved`. `Syntax` associates a supplier with a canonical node; `Span` is the precise supplier region where available. `Element.Input` identifies the collection supplying an item. `Reason` explains unresolved or external-provider cases. External input and unresolved sources have no invented document span.

For `.address.city`, the `.city` selection reads the result of `.address`. Both selections retain the same canonical reference node, with distinct selection spans and source regions. Offsets use the canonical syntax model's zero-based UTF-16 positions and lengths, including across lines.

Consumers that already parse canonical syntax can preserve node identity:

```csharp
using Expressif.Syntax;
using Expressif.Semantics;

var syntax = ExpressifSyntax.Parse(text);
var analysis = new SemanticAnalyzer().Analyze(syntax);
```

The string overload uses `ExpressifSyntax.Parse` directly. It does not apply the runtime adapter's source-rewriting compatibility shorthands, such as vector-constructor normalization. Pass original canonical syntax when matching results to an editor document; spans from an already rewritten syntax tree refer to that rewritten text.

## Scope behavior

Pipelines advance the input while retaining the current expression root. Mapping and filtering follow the runtime's nested-expression construction, including the existing distinction between a single predicate and a predicate pipeline. `with`, record projections, and `apply` retain their input and argument contexts.

For `map-over`, the operation's pipeline input is the outer input and its argument context is the iterated item. For `map-with`, the operation's pipeline input is the iterated item; explicit argument expressions also use that item. A bare callable in `map-with` receives the outer input as its implicit argument. Named arguments are resolved through the same parameter binder used by runtime construction.

Runtime construction and analysis share evaluation-frame derivation, root selection, argument-context selection, directional-map input selection, and the classification of factory-integrated calls. The analysis does not infer these rules from documentation or input types.

## Partial and unsupported analysis

Invalid syntax returns diagnostics and an empty reference list. Binding failures return diagnostics and unresolved references from the available canonical tree. References inside unsupported scope transitions remain present with `Source.Kind == Unresolved`; consumers should not highlight a supplier for them.

This API is scope analysis, not a complete expression validator or type checker. Other factory-integrated operators, custom higher-order operators, and multi-argument Boolean combinators may produce unresolved results. Function calls such as `field("name")` and computed field names are not canonical field-reference nodes and are not included in `References`. No field declarations, runtime values, or schema members are inferred. Ordinary runtime null handling still applies during evaluation; analysis describes the structural source relationships without testing runtime values.

The analyzer can be reused; each invocation owns its binding map and analysis state. The API has no LSP or editor-framework dependency.
