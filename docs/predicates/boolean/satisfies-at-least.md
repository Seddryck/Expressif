---
layout: docs
title: "satisfies-at-least"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 130
has_toc: false
permalink: /predicates/boolean/satisfies-at-least/
tags:
  - predicates
  - boolean
generated: true
---

```
any →
satisfies-at-least(
    count: integer,
    ...predicates: predicate
) → boolean
```

Returns `true` when at least the requested number of supplied predicates are satisfied by the input. The count must be non-negative, and evaluation stops as soon as the result is known.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Specifies the minimum non-negative number of predicates that must be satisfied. |
| `predicates` | `predicate` | Variadic (zero or more) | Specifies the predicate expressions evaluated against the same input value, in declaration order. |






## Examples

{% raw %}
```expressif
4 | satisfies-at-least(2, is-positive, is-even, is-greater-than(10)) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
