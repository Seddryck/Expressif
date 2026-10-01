---
layout: docs
title: "sum"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 170
has_toc: false
permalink: /functions/array/aggregation/sum/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
sum() → any
```

Computes the sum of all accumulated numeric values.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `numeric`
- **Returns:** `numeric`





## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.





## Examples

{% raw %}
```expressif
{10, 20, 30} | fold(sum) → 60
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
