---
layout: docs
title: "every"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 60
has_toc: false
permalink: /accumulators/array/every/
tags:
  - accumulators
  - array
generated: true
---

```
array →
every() → any
```

Returns `true` only when every accumulated boolean value is `true`.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{#true, #true, #false} | fold(every) → #false
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
