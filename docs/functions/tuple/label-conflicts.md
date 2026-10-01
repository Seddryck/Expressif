---
layout: docs
title: "label-conflicts"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/tuple/label-conflicts/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
label-conflicts(
    ...names: text
) → record
```

Returns a flat record whose expanded record fields receive positional labels only when their unqualified names conflict.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `names` | `text` | Variadic (zero or more); no spread | One label for each tuple position, in positional order. Omission supplies an empty variadic sequence. |



## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each position of the tuple supplied as pipeline input to this label-conflicts call in positional order. A record item contributes its fields in source order, while every other item contributes one field.

- **`names`:** Each supplied label is evaluated once in the enclosing context before the tuple positions are visited.


## Behavior

The number of labels must equal the tuple arity. A scalar or nested tuple is emitted under its positional label. A record is expanded one level: a field keeps its original name when that name occurs once among all fields that would otherwise be emitted, and conflicting record fields are named `<label>.<field>`. Nested tuples remain atomic. Tuple positions and source-record fields retain their original order. Empty tuples accept no labels and return an empty record. Only positional label arguments are accepted; spread arguments are not supported. An arity mismatch or duplicate resulting field name raises an evaluation error.



## Examples

{% raw %}
```expressif
T(42, {key := 1, value := "Brol"}, "A", {key := 2, score := 10}) | label-conflicts("id", "left", "grade", "right") → {id := 42, "left.key" := 1, value := "Brol", grade := "A", "right.key" := 2, score := 10}
({key := 1, value := "Brol"} => {key := 1, grade := "A"}) | label-conflicts("left", "right") → {"left.key" := 1, value := "Brol", "right.key" := 1, grade := "A"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** None
{: .member-reference }
