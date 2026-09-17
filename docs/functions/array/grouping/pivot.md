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
| `row` | `expression` | Yes | A direct field selector defining row keys and the output row field name. |
| `column` | `expression` | Yes | An expression defining column keys, which are coerced to output field names. |
| `summary` | `expression` | Yes | An expression transforming the generated grouping into a dictionary with the same composite keys. |



## Argument evaluation

Visits each element of the array supplied as pipeline input to this pivot call, in source order.

- **`row`:** Evaluated once per visited element before column, with that element as its context. The selected field is read from that element.
- **`column`:** Evaluated once per visited element after row, with that element as its context; .field reads that element's field and $0 refers to its first tuple position.
- **`summary`:** Evaluated once after all grouping keys have been evaluated, with the complete generated grouping as its context, including for an empty array. Each grouping key is T(row, column), in that order; summarize defines its own group selector and accumulator contexts.


## Behavior

Equivalent to group-by(row, column), followed by summary, nest, and conversion of each inner dictionary through from-pairs with the row field prepended. The row must be a direct field selector such as .country; computed or unnamed row expressions fail during binding. The summary must return a dictionary preserving every generated composite key; incompatible output fails during evaluation. Row and column order follow the summary dictionary and nest insertion order. Missing combinations produce absent fields. Column keys use from-pairs text coercion; uncoercible keys, duplicate coerced names, and collisions with the row field fail during evaluation.



## Examples

{% raw %}
```expressif
{{country := "BE", year := 2025, amount := 30}, {country := "BE", year := 2025, amount := 70}, {country := "BE", year := 2026, amount := 120}, {country := "FR", year := 2025, amount := 80}} | pivot(.country, .year, summarize(|> .amount | sum)) → {{country := "BE", "2025" := 100, "2026" := 120}, {country := "FR", "2025" := 80}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/grouping`  
**Aliases:** None
{: .member-reference }
