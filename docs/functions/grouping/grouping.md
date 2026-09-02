---
layout: docs
title: "grouping"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/grouping/grouping/
tags:
  - functions
  - grouping
generated: true
---

```
any →
grouping(
    ...values: pair
) → grouping
```

Constructs a grouping from zero or more pairs. Spread arguments expand arrays of pairs in place.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `values` | `pair` | Variadic (zero or more) | Zero or more pairs whose keys and grouped value collections become groups. |






## Examples

{% raw %}
```expressif
#null | grouping() → #{}
#null | grouping(("BE" => {"alice", "bob"}), ("FR" => {"charlie"})) → #{("BE" => {"alice", "bob"}), ("FR" => {"charlie"})}
#null | grouping(...{("BE" => {"alice"}), ("FR" => {"charlie"})}) → #{("BE" => {"alice"}), ("FR" => {"charlie"})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
