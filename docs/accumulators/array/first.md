---
layout: docs
title: "first"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 40
has_toc: false
permalink: /accumulators/array/first/
tags:
  - accumulators
  - array
generated: true
---

```
first()
```

Stores the first accumulated item and ignores all subsequent items.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{10, 20, 30} | fold(first) → 10
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `first`
{: .member-reference }
