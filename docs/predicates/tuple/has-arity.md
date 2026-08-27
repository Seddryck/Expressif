---
layout: docs
title: "has-arity"
parent: "Tuple predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/tuple/has-arity/
tags:
  - predicates
  - tuple
generated: true
---

```
tuple →
has-arity(
    expected: integer
) → boolean
```

Returns whether the input tuple has exactly the expected number of positions.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expected` | `integer` | Yes | Specifies the required non-negative tuple arity. |






## Examples

{% raw %}
```expressif
T(1, 2) | has-arity(2) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `tuple`  
**Aliases:** None
{: .member-reference }
