---
layout: docs
title: "explode-outer"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/record/explode-outer/
tags:
  - functions
  - record
generated: true
---

```
array | record →
explode-outer(
    selector: expression
) → array
```

Emits one record per selected collection element, preserving parents with empty or null fields.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `selector` | `expression` | Yes | A direct field selector identifying the collection-valued field to replace. |



## Argument evaluation

Visits the record supplied as pipeline input to this explode-outer call, or each parent record of its input array in source order. Visits selected children in source order, retaining a parent with no children.

- **`selector`:** Evaluated once per parent record before visiting its children, with that parent as context. .field reads that parent's field, without retaining the surrounding context; an empty source array evaluates no selector.


## Behavior

Matches explode for non-empty collections, preserving field position, child order, duplicates, and one-level expansion. Empty, null, or missing selected fields emit one parent with a null selected field; a missing field is appended. Non-collection selected values and non-record parents cause an evaluation error. Null pipeline input returns null. The selector must be a direct field reference; computed expressions and nested paths fail during binding. Empty and originally null collections become indistinguishable. expand changes record shape without expanding cardinality.



## Examples

{% raw %}
```expressif
{id := 1, tags := {}} | explode-outer(.tags) → {{id := 1, tags := #null}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
