---
layout: docs
title: "satisfies-exactly"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 150
has_toc: false
permalink: /predicates/boolean/satisfies-exactly/
tags:
  - predicates
  - boolean
generated: true
---

```
any →
satisfies-exactly(
    count: integer,
    ...predicates: predicate
) → boolean
```

Returns `true` when exactly the requested number of supplied predicates are satisfied by the input. The count must be non-negative, and evaluation stops as soon as the result is known.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Specifies the exact non-negative number of predicates that must be satisfied. |
| `predicates` | `predicate` | Variadic (zero or more) | Specifies the predicate expressions evaluated against the same input value, in declaration order. |






## Examples

{% raw %}
```expressif
4 | satisfies-exactly(2, is-positive, is-even, is-greater-than(10)) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
