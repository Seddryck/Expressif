---
layout: docs
title: "satisfies-at-most"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 140
has_toc: false
permalink: /predicates/boolean/satisfies-at-most/
tags:
  - predicates
  - boolean
generated: true
---

```
any →
satisfies-at-most(
    count: integer,
    ...predicates: predicate
) → boolean
```

Returns `true` when at most the requested number of supplied predicates are satisfied by the input. The count must be non-negative, and evaluation stops as soon as the result is known.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Specifies the maximum non-negative number of predicates that may be satisfied. |
| `predicates` | `predicate` | Variadic (zero or more) | Specifies the predicate expressions evaluated against the same input value, in declaration order. |






## Examples

{% raw %}
```expressif
4 | satisfies-at-most(2, is-positive, is-even, is-greater-than(10)) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
