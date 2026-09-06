---
layout: docs
title: "dot"
parent: "Vector functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/vector/dot/
tags:
  - functions
  - vector
generated: true
---

```
vector ΓåÆ
dot(
    vector: vector
) ΓåÆ numeric
```

Returns the dot product of the input vector and another vector of the same dimension.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `vector` | `vector` | Yes | Specifies the vector whose components are multiplied with the input components. |






## Examples

{% raw %}
```expressif
V(1, 2, 3) | dot(V(4, 5, 6)) ΓåÆ 32
```
{% endraw %}


**Kind:** Function  
**Scope:** `vector`  
**Aliases:** None
{: .member-reference }
