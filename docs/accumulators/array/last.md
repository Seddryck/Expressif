---
layout: docs
title: "last"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 50
has_toc: false
permalink: /accumulators/array/last/
tags:
  - accumulators
  - array
generated: true
---

```
last()
```

Stores the most recently accumulated item.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{10, 20, 30} | fold(last) → 30
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `last`
{: .member-reference }
