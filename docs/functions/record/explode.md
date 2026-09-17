---
layout: docs
title: "explode"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/record/explode/
tags:
  - functions
  - record
generated: true
---

```
array | record →
explode(
    selector: expression
) → array
```

Emits one record per element of a selected collection-valued field, preserving other fields and field order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `selector` | `expression` | Yes | A direct field selector identifying the collection-valued field to replace. |



## Argument evaluation

Visits the record supplied as pipeline input to this explode call, or each parent record of its input array in source order. Visits the selected collection children in their order, removing one collection level.

- **`selector`:** Evaluated once per parent record before visiting its children, with that parent as context. .field reads that parent's field, without retaining the surrounding context; an empty source array evaluates no selector.


## Behavior

Accepts one record or an array of records, preserving parent order, child order, and duplicates. Replaces the selected field in its original position and removes exactly one collection level. Empty, null, or missing selected fields emit no rows; null children remain children. Non-collection selected values and non-record parents cause an evaluation error; text is never parsed as a collection. The selector must be a direct field reference; computed expressions and nested paths fail during binding. Null pipeline input returns null. explode-outer adds parent preservation for empty or null fields. Structural implode recomposes exploded fields, while implode-inner removes null placeholders. expand flattens record shape without expanding cardinality.



## Examples

{% raw %}
```expressif
{id := 1, tags := {"A", "B"}} | explode(.tags) → {{id := 1, tags := "A"}, {id := 1, tags := "B"}}
{id := 1, tags := {}} | explode(.tags) → {}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
