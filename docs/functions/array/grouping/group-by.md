---
layout: docs
title: "group-by"
parent: "Grouping functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/grouping/group-by/
tags:
  - functions
  - array/grouping
generated: true
---

```
array →
group-by(
    ...expressions: expression
) → grouping
```

Groups input values by keys calculated from one or more expressions.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expressions` | `expression` | Variadic (one or more) | One or more expressions evaluated once per input value; multiple results form a tuple key. |






## Examples

{% raw %}
```expressif
{"BE", "be", "FR"} | group-by(lower) → #{("be" => {"BE", "be"}), ("fr" => {"FR"})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/grouping`  
**Aliases:** None
{: .member-reference }
