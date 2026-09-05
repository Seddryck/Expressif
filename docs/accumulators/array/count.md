---
layout: docs
title: "count"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 20
has_toc: false
permalink: /accumulators/array/count/
tags:
  - accumulators
  - array
generated: true
---

```
count()
```

Counts the number of accumulated items, including `null` values.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{10, #null, 30} | fold(count) → 3
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `count`
{: .member-reference }
