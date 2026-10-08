---
layout: docs
title: "unpivot"
parent: "Grouping functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/grouping/unpivot/
tags:
  - functions
  - array/grouping
generated: true
---

```
array →
unpivot(
    fields: array,
    name-field: text,
    value-field: text
) → array
```

Converts selected present record fields into rows, preserving retained fields and source order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `fields` | `array` | Yes | An ordered array of distinct text field names to turn into rows. |
| `name-field` | `text` | Yes | The output field containing the selected source field name. |
| `value-field` | `text` | Yes | The output field containing the selected source field value. |



## Examples

{% raw %}
```expressif
{{id := 1, x := #null}} | unpivot({"x"}, "name", "value") → {{id := 1, name := "x", value := #null}}
```
{% endraw %}

## Structural semantics

- Cardinality: `expanded` <span class="semantics-info" title="One visited input can produce multiple output elements." aria-label="Cardinality definition: One visited input can produce multiple output elements.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each record of the array supplied as pipeline input to this unpivot call, in source order; selected field names determine row order within each record.

- **`fields`:** Evaluated once before name-field in the enclosing context of this call; .field reads that enclosing record, rather than a visited source record.
- **`name-field`:** Evaluated once after fields and before value-field in the enclosing context of this call.
- **`value-field`:** Evaluated once after name-field in the enclosing context of this call. All three arguments run before source records are visited, including for empty arrays.


## Behavior

Removes all selected fields from each source record and emits one row per present selected field in selection order. Retained fields keep their order, followed by name-field and value-field. Explicit nulls produce rows; absent fields do not. Empty input or selection returns an empty array. Source items must be records. Selected names must be distinct text values; output names must be non-null and distinct. Output names may reuse selected names but must not collide with retained fields; conflicts use the from-pairs duplicate-field diagnostic. Names are case-sensitive. This is the tabular counterpart of pivot, subject to its aggregation and field-name coercion. Unlike expand, which flattens nested records, unpivot reshapes fields into rows. Like flat-map, it is a one-to-many transformation.

**Kind:** Function  
**Scope:** `array/grouping`  
**Aliases:** None
{: .member-reference }
