---
layout: docs
title: "filter-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/grouping/filter-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
filter-groups(
    predicate: predicate
) → grouping
```

Keeps whole groups whose group-level predicate evaluates to true.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicate` | `predicate` | Yes | The predicate evaluated once against each group, with its key and value collection available. |






## Examples

{% raw %}
```expressif
#{("BE" => {1, 2}), ("FR" => {3})} | filter-groups($value | cardinality | greater-than(1)) → #{("BE" => {1, 2})}
#{("BE" => {1}), ("FR" => {2})} | having($key | is-equivalent-to("FR")) → #{("FR" => {2})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** `having`
{: .member-reference }
