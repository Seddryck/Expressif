---
layout: docs
title: "drill-up"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/grouping/drill-up/
tags:
  - functions
  - grouping
generated: true
---

```
grouping<K, T> →
drill-up(
    expression: expression
) → grouping<U, T>
```

Derives keys from existing grouping keys and merges matching groups into one grouping level.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | The expression that derives a new key from each existing group key. |



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

## Value shape

- Pipeline input: `grouping<K, T>`
- Returns: `grouping<U, T>`
- `expression`: Receives `K` and returns `U`.
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each existing key of the grouping supplied as pipeline input to this drill-up call, in group order, including keys of empty groups.

- **`expression`:** Evaluated once per existing key, with that key as its context; $0 reads the first component of a tuple key and .field reads a record key's field.


## Behavior

Structural equality includes null and tuple keys. Matching keys merge value collections in source-group order; new keys retain first-seen order, including keys from empty groups. Returns one grouping level, unlike roll-up, which produces multiple hierarchical levels.

### Combine annual order lists into country lists

The first example groups order IDs by `(country, year)`. `drill-up($0)` reads the country from each existing tuple key and combines the Belgian order lists across years. The order IDs remain unchanged and in source-group order; they contain no year field to re-evaluate.

### Calculate country revenue across all years

The second example keeps individual order amounts under `(country, year)` keys. `drill-up($0)` first merges the amounts for each country, and `summarize(sum)` then calculates the totals: Belgium has `120 + 30 + 80 = 230`, and France has `90`. `$0` refers to the country component of each existing key, not to an amount. This produces one country level; use `roll-up` when multiple hierarchical levels are needed.

**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
