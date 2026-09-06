---
layout: docs
title: "distance"
parent: "Vector functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/vector/distance/
tags:
  - functions
  - vector
generated: true
---

```
vector →
distance(
    vector: vector
) → numeric
```

Returns the Euclidean distance between the input vector and another vector of the same dimension.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `vector` | `vector` | Yes | Specifies the vector whose distance from the input vector is calculated. |






## Examples

{% raw %}
```expressif
V(1, 2) | distance(V(4, 6)) → 5
```
{% endraw %}


**Kind:** Function  
**Scope:** `vector`  
**Aliases:** None
{: .member-reference }
