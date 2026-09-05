---
layout: docs
title: "is-absent"
parent: "Record predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/record/is-absent/
tags:
  - predicates
  - record
generated: true
---

```
record →
is-absent(
    name: text
) → boolean
```

Returns whether the named field does not exist in the input record, independently of field values.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `name` | `text` | Yes | Name of the field whose absence is tested. |






## Examples

{% raw %}
```expressif
{name := "Alice"} | is-absent(age) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
