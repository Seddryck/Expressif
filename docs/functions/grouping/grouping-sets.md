---
layout: docs
title: "grouping-sets"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/grouping/grouping-sets/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
grouping-sets(
    ...values: tuple
) → grouping
```

Expands a grouping into explicitly declared sets of retained key dimensions.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `values` | `tuple` | Variadic (zero or more); accepts spread | Zero or more tuples of zero-based key dimension positions to retain. Omission supplies an empty variadic sequence. |



## Structural semantics

- **Cardinality:** `expanded`
- **Dependency:** `partition`
- **Ordering:** `preserved`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

## Argument evaluation

Visits each group of the grouping supplied as pipeline input to this grouping-sets call in source order for each distinct declared level. Existing keys determine dimensions; grouped values are copied without evaluating expressions against them.

- **`values`:** Each supplied set expression is evaluated once in declaration order against the complete grouping supplied as pipeline input to this grouping-sets call. Its integer results select existing key dimensions; $0 does not refer to a group's key here.


## Behavior

Each set is a tuple of zero-based integer dimension positions: T(0, 1) retains the first two dimensions, tuple(0) retains only the first, and tuple() requests the grand total. Positions describe existing key components, not expressions evaluated against grouped values. Declaration order determines level order; component order stays unchanged. Equivalent sets, including reordered or repeated positions, are emitted once at their first declaration. No arguments returns an empty grouping. Spread accepts an array of set tuples in place. Named arguments are not supported. Negative, fractional, nonnumeric and out-of-range positions fail with an argument error; non-tuple sets also fail. For an empty grouping there are no dimensions, so only empty sets are valid. Scalar keys have one dimension at position zero; arrays and records are scalar dimensions, and nested tuples remain single dimensions. Keys must have consistent scalar/tuple shape and tuple arity and must not already contain #all as a dimension. Unretained dimensions use the same #all marker as roll-up and cube, distinct from real null keys. Within each level, keys retain first-seen order and values concatenate in source-group order, including duplicates, nulls and empty groups. Values are preserved without summarization.



## Examples

{% raw %}
```expressif
#{(T("BE", 2025) => {1}), (T("BE", 2026) => {2})} | grouping-sets(tuple(0), tuple()) → #{(T("BE", #all) => {1, 2}), (T(#all, #all) => {1, 2})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
