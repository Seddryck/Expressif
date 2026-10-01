---
layout: docs
title: "implode-inner"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/record/implode-inner/
tags:
  - functions
  - record
generated: true
---

```
array →
implode-inner(
    selector: expression
) → array
```

Groups records by all non-selected fields and collects non-null selected values in source order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `selector` | `expression` | Yes | A direct field selector identifying the field whose values are collected. |



## Examples

{% raw %}
```expressif
{{id := 1, tags := "A"}, {id := 1, tags := #null}} | implode-inner(.tags) → {{id := 1, tags := {"A"}}}
```
{% endraw %}

## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each record of the array supplied as pipeline input to this implode-inner call in source order, grouping by every non-selected field.

- **`selector`:** Evaluated once per source record, with that record as context. .field reads that record's field without retaining the surrounding context; an empty source array evaluates no selector.


## Behavior

Uses the same structural parent grouping and field shaping as implode, excluding null and DBNull selected values while retaining null-only parent groups with empty arrays. Parent identity is the complete ordered record after removing the selected field, using Expressif structural value equality including nested records, tuples, and arrays. Preserves first-seen group order, first-parent field order, child order, and duplicates. Missing selected fields are treated as null and appended to the output. Empty input returns an empty array; null pipeline input returns null. Non-record elements and scalar or record pipeline input cause an evaluation error. Selectors must be direct field references; computed expressions and nested paths fail during binding. Does not recursively collect nested structures. explode-outer followed by implode-inner restores empty child collections, but also turns originally null collections and literal null children into empty collections, so it is not a perfect inverse. expand changes record shape without changing cardinality.

**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
