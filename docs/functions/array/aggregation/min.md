---
layout: docs
title: "min"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 130
has_toc: false
permalink: /functions/array/aggregation/min/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
min() → any
```

Tracks the smallest numeric value found during accumulation.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `numeric`
- **Returns:** `nullable<numeric>`





## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.





## Examples

{% raw %}
```expressif
{10, 30, 20} | fold(min) → 10
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
