---
layout: docs
title: "sum"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 90
has_toc: false
permalink: /accumulators/array/sum/
tags:
  - accumulators
  - array
generated: true
---

```
sum()
```

Computes the sum of all accumulated numeric values.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{10, 20, 30} | fold(sum) → 60
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `sum`
{: .member-reference }
