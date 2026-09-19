---
layout: docs
title: "pivot"
parent: "Grouping functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/grouping/pivot/
tags:
  - functions
  - array/grouping
generated: true
---

```
array →
pivot(
    row: expression,
    column: expression,
    summary: expression
) → array
```

Groups values by row and column, applies a grouping summary, and reshapes the cells into records.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `row` | `expression` | Yes | A direct field selector or tuple of direct field selectors defining row dimensions and their output field names. |
| `column` | `expression` | Yes | An expression defining column keys, which are coerced to output field names. |
| `summary` | `expression` | Yes | An expression transforming the generated grouping into a dictionary with the same composite keys. |



## Structural semantics

- **Cardinality:** `partitioned`
- **Dependency:** `partition`
- **Ordering:** `preserved`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

## Argument evaluation

Visits each element of the array supplied as pipeline input to this pivot call, in source order.

- **`row`:** Evaluated once per visited element before column, with that element as its context. Each selected field is read from that element; tuple dimensions are evaluated in declaration order.
- **`column`:** Evaluated once per visited element after row, with that element as its context; .field reads that element's field and $0 refers to its first tuple position.
- **`summary`:** Evaluated once after all grouping keys have been evaluated, with the complete generated grouping as its context, including for an empty array. Each grouping key contains the row dimensions followed by the column, such as T(country, category, year) for row T(.country, .category) and column .year; summarize defines its own group selector and accumulator contexts.


## Behavior

Equivalent to grouping by each row dimension followed by column, applying summary, nesting, and converting the innermost dictionaries through from-pairs with row fields prepended. The row must be a direct field selector such as .country or a nonempty tuple of direct field selectors such as T(.country, .category). Tuple positions become separate named row dimensions in declaration order; computed, unnamed, spread, or duplicate row dimensions fail during binding. Arrays and records selected as row fields remain single structural keys. Tuple-valued individual row fields and tuple-valued columns fail during evaluation. The summary must return a dictionary preserving every generated composite key; incompatible output fails during evaluation. Row and column order follow the summary dictionary and nest insertion order. Missing combinations produce absent fields. Column keys use from-pairs text coercion; uncoercible keys, duplicate coerced names, and collisions with any row field fail during evaluation.



## Examples

{% raw %}
```expressif
{{country := "BE", category := "Retail", year := 2025, amount := 100}} | pivot(T(.country, .category), .year, summarize(|> .amount | sum)) → {{country := "BE", category := "Retail", "2025" := 100}}
{{country := "BE", category := "Retail", city := "Brussels", year := 2025}} | pivot(T(.country, .category, .city), .year, summarize(cardinality)) → {{country := "BE", category := "Retail", city := "Brussels", "2025" := 1}}
{{country := "BE", year := 2025, amount := 30}, {country := "BE", year := 2025, amount := 70}, {country := "BE", year := 2026, amount := 120}, {country := "FR", year := 2025, amount := 80}} | pivot(.country, .year, summarize(|> .amount | sum)) → {{country := "BE", "2025" := 100, "2026" := 120}, {country := "FR", "2025" := 80}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/grouping`  
**Aliases:** None
{: .member-reference }
