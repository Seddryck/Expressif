---
layout: docs
title: "roll-up"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/grouping/roll-up/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
roll-up() → grouping
```

Expands a grouping into its original level and progressively coarser prefix levels.



## Parameters



This function has no parameters.



## Argument evaluation

Visits each group of the grouping supplied as pipeline input to this roll-up call in source order for each prefix level, including empty groups. Existing keys determine dimensions; grouped values are copied without evaluating expressions against them.



## Behavior

Removes dimensions from right to left, retaining tuple arity. Scalar keys have one dimension and produce a scalar total key; arrays and records are scalar dimensions. Empty groupings remain empty, and zero-component tuples have only their original level. All keys must be scalar or all tuples of the same arity; mixed shapes fail with an argument error. Nested tuples remain single dimensions. The dedicated AllDimension.Instance value represents an aggregated dimension and is displayed as #all, distinct from null and the string "#all". This display token is not a source literal. Keys already containing this marker as a dimension are rejected to prevent double-counting. Original groups come first, then successive prefix levels, ending with the grand total. Within each level, keys retain first-seen order and values concatenate in source-group order, including duplicates, nulls and empty groups. Values are never aggregated; compose with summarize to calculate totals.



## Examples

{% raw %}
```expressif
#{(T("BE", 2025) => {120, 30}), (T("FR", 2025) => {90}), (T("BE", 2026) => {80})} | roll-up | summarize(sum) | map(pair-value) → {150, 90, 80, 230, 90, 320}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
