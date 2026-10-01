---
layout: docs
title: "top-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 120
has_toc: false
permalink: /functions/grouping/top-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping<K, T> →
top-groups(
    count: integer,
    expression: expression
) → grouping<K, T>
```

Keeps up to count complete groups in descending ranking order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The maximum number of groups to select. |
| `expression` | `expression` | Yes | The expression that supplies each group's ranking score. |



## Value shape

- Pipeline input: `grouping<K, T>`
- Returns: `grouping<K, T>`
- `expression`: Receives `pair<K, array<T>>` and returns `S`.
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each group of the grouping supplied as pipeline input to this top-groups call, including empty groups, when count is positive.

- **`count`:** Evaluated once in the surrounding context before any ranking expressions.
- **`expression`:** For a positive count, evaluated once per group in source order with that Group as context: $key reads its existing key and $value reads its complete value collection. It is not evaluated for zero count.


## Behavior

Ranks comparable scalar scores using the same numeric normalization and ordinal text ordering as min-by and max-by. Null scores sort last, as in normal descending sorting. Equal scores retain source-group order. Zero count returns an empty grouping without evaluating scores; negative counts fail with an argument error. Oversized counts return all groups in ranking order. Values are preserved without summarization.



## Examples

{% raw %}
```expressif
#{("BE" => {1}), ("FR" => {2, 3})} | top-groups(1, $value | cardinality) → #{("FR" => {2, 3})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
