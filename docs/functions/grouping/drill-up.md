---
layout: docs
title: "drill-up"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/grouping/drill-up/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
drill-up(
    expression: expression
) → grouping
```

Derives keys from existing grouping keys and merges matching groups into one grouping level.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | The expression that derives a new key from each existing group key. |



## Argument evaluation

Visits each existing key of the grouping supplied as pipeline input to this drill-up call, in group order, including keys of empty groups.

- **`expression`:** Evaluated once per existing key, with that key as its context; $0 reads the first component of a tuple key and .field reads a record key's field.


## Behavior

Structural equality includes null and tuple keys. Matching keys merge value collections in source-group order; new keys retain first-seen order, including keys from empty groups. Returns one grouping level, unlike roll-up, which produces multiple hierarchical levels.

### Combine annual order lists into country lists

The first example groups order IDs by `(country, year)`. `drill-up($0)` reads the country from each existing tuple key and combines the Belgian order lists across years. The order IDs remain unchanged and in source-group order; they contain no year field to re-evaluate.

### Calculate country revenue across all years

The second example keeps individual order amounts under `(country, year)` keys. `drill-up($0)` first merges the amounts for each country, and `summarize(sum)` then calculates the totals: Belgium has `120 + 30 + 80 = 230`, and France has `90`. `$0` refers to the country component of each existing key, not to an amount. This produces one country level; use `roll-up` when multiple hierarchical levels are needed.



## Examples

{% raw %}
```expressif
#{
  (T("BE", 2025) => {"BE-001", "BE-002"}),
  (T("FR", 2025) => {"FR-001"}),
  (T("BE", 2026) => {"BE-003"})
}
| drill-up($0)
→ #{("BE" => {"BE-001", "BE-002", "BE-003"}), ("FR" => {"FR-001"})}

#{
  (T("BE", 2025) => {120, 30}),
  (T("FR", 2025) => {90}),
  (T("BE", 2026) => {80})
}
| drill-up($0)
| summarize(sum)
→ !{("BE" => 230), ("FR" => 90)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
