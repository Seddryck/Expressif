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



## Examples

{% raw %}
```expressif
#{(T("BE", 2025) => {1}), (T("BE", 2026) => {2})} | drill-up($0) → #{("BE" => {1, 2})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
