---
layout: docs
title: "exists"
parent: "Combination predicates"
grand_parent: "Array predicates"
nav_order: 10
has_toc: false
permalink: /predicates/array/combination/exists/
tags:
  - predicates
  - array/combination
generated: true
---

```
any →
exists(
    right: array | grouping,
    left-key: expression,
    right-key?: expression
) → boolean
```

Returns whether the input value has a matching key in the supplied array or grouping.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `array | grouping` | Yes | The array or grouping supplying matching keys. |
| `left-key` | `expression` | Yes | Selects the lookup key of the input value. |
| `right-key` | `expression` | No | Selects each right array value’s key; when omitted, the left-key expression is reused. It is skipped for a grouping. |



## Structural semantics

- **Cardinality:** `collapsed`
- **Dependency:** `whole-input`
- **Ordering:** `not-applicable`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

## Argument evaluation

Visits elements of the array supplied as right in order until a matching key is found. For a grouping, checks bucket keys without visiting bucket values.

- **`right`:** Evaluated once in the enclosing context of this exists call, before evaluating the input key.
- **`left-key`:** Evaluated once with the pipeline input to this exists call as its context. When right-key is omitted for an array, the same expression is also evaluated with each visited right element as its context.
- **`right-key`:** Evaluated with each visited element of the array supplied as right as its context, stopping at the first match. Skipped when right is a grouping.


## Behavior

Uses structural key equality, including null keys. Empty arrays and groupings return false; a grouping key exists even when its bucket is empty. A null or unsupported right value returns false.



## Examples

{% raw %}
```expressif
{{id := 1}, {id := 2}} | filter(exists({{customer-id := 1}}, .id, .customer-id)) → {{id := 1}}
1 | exists(#{(1 => {})}, @_) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
