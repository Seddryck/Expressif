---
layout: docs
title: "implode"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 90
has_toc: false
permalink: /functions/record/implode/
tags:
  - functions
  - record
generated: true
---

```
array →
implode(
    selector: expression
) → array
```

Groups records by all non-selected fields and collects selected values in source order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `selector` | `expression` | Yes | A direct field selector identifying the field whose values are collected. |



## Examples

{% raw %}
```expressif
{{id := 1, tags := "A"}, {id := 1, tags := #null}, {id := 1, tags := "B"}} | implode(.tags) → {{id := 1, tags := {"A", #null, "B"}}}
```
{% endraw %}

## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each record of the array supplied as pipeline input to this implode call in source order, grouping by every non-selected field.

- **`selector`:** Evaluated once per source record, with that record as context. .field reads that record's field without retaining the surrounding context; an empty source array evaluates no selector.


## Behavior

Groups source records by the complete ordered record remaining after the selected field is removed, using Expressif structural value equality including nested records, tuples, and arrays. Replaces the selected field in the first record of each group with an array containing every selected value in source order, preserving duplicates, null, and DBNull values. Missing selected fields are treated as null and collected. Preserves first-seen group order and first-parent field order. Empty input returns an empty array; null pipeline input returns null. Non-record elements and scalar or record pipeline input cause an evaluation error. Selectors must be direct field references; computed expressions and nested paths fail during binding. Does not recursively collect nested structures. For non-empty collection-valued fields, explode followed by implode reconstructs the original field values; strict explode loses empty collections, while explode-outer can make empty and null-like states indistinguishable. implode-inner removes null placeholders. expand changes record shape without changing cardinality.

**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
