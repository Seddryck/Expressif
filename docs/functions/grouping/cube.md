---
layout: docs
title: "cube"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/grouping/cube/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
cube() → grouping
```

Expands a grouping into all combinations of retained and aggregated dimensions.



## Parameters



This function has no parameters.



## Argument evaluation

Visits each group of the grouping supplied as pipeline input to this cube call in source order for each subset of dimensions, including empty groups. Existing keys determine dimensions; grouped values are copied without evaluating expressions against them.



## Behavior

Generates all 2^N grouping levels for N dimensions, starting with the original level and ending with the grand total. Levels follow binary mask order, with the rightmost dimension changing fastest: for two dimensions, (a, b), (a, #all), (#all, b), then (#all, #all). The number of levels grows exponentially with the number of dimensions. Within each level, keys retain first-seen order and values concatenate in source-group order, including duplicates, nulls and empty groups. Values are never aggregated; compose with summarize to calculate totals.

Uses the same AllDimension.Instance marker as roll-up, displayed as #all and distinct from null and the string "#all". This display token is not a source literal. Scalar keys have one dimension and a scalar total key; arrays and records are scalar dimensions. Tuple arity is retained, with nested tuples treated as single dimensions. Empty groupings remain empty; zero-component tuples have only their original level. All keys must be scalar or all tuples of the same arity. Mixed shapes and keys already containing the aggregated marker as a dimension fail with an argument error, preventing ambiguous dimensions and double-counting.



## Examples

{% raw %}
```expressif
#{(T("BE", 2025) => {120, 30}), (T("FR", 2025) => {90}), (T("BE", 2026) => {80})} | cube | summarize(sum) | map(pair-value) → {150, 90, 80, 230, 90, 240, 80, 320}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
