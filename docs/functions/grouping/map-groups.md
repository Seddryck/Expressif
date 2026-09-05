---
layout: docs
title: "map-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/grouping/map-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
map-groups(
    expression: expression
) → grouping
```

Transforms each group's value collection while preserving its key and position.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | The expression evaluated once against each group's value collection. |






## Examples

{% raw %}
```expressif
#{("BE" => {10, 20, 30}), ("FR" => {5, 15})} | map-groups(filter(greater-than(10))) → #{("BE" => {20, 30}), ("FR" => {15})}
#{("BE" => {10}), ("FR" => {20})} |#> filter(greater-than(15)) → #{("BE" => {}), ("FR" => {20})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
