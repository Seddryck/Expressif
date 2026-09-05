---
layout: docs
title: "min"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 70
has_toc: false
permalink: /accumulators/array/min/
tags:
  - accumulators
  - array
generated: true
---

```
min()
```

Tracks the smallest numeric value found during accumulation.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{10, 30, 20} | fold(min) → 10
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `min`
{: .member-reference }
