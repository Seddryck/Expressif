---
layout: docs
title: "max"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 60
has_toc: false
permalink: /accumulators/array/max/
tags:
  - accumulators
  - array
generated: true
---

```
max()
```

Tracks the greatest numeric value found during accumulation.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{10, 30, 20} | fold(max) → 30
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `max`
{: .member-reference }
