---
layout: docs
title: "majority"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 80
has_toc: false
permalink: /predicates/boolean/majority/
tags:
  - predicates
  - boolean
generated: true
---

```
any →
majority(
    ...predicates: predicate
) → boolean
```

Returns `true` when strictly more than half of the supplied predicates are satisfied by the input. Returns `false` when no predicates are supplied and stops evaluating as soon as the result is known.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicates` | `predicate` | Variadic (zero or more) | Specifies the predicate expressions evaluated against the same input value, in declaration order. |

## Argument evaluation

- **`predicates`:** Predicates use the same incoming value and are evaluated in declaration order. Evaluation stops as soon as the result is known.

## Examples

{% raw %}
```expressif
4 | majority(is-positive, is-even, is-less-than(10)) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
