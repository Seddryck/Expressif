---
layout: docs
title: "is-present"
parent: "Record predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/record/is-present/
tags:
  - predicates
  - record
generated: true
---

```
record →
is-present(
    name: text
) → boolean
```

Returns whether the named field exists in the input record, independently of its value.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `name` | `text` | Yes | Name of the field whose presence is tested. |






## Examples

{% raw %}
```expressif
{name := "Alice", age := #null} | is-present(age) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
