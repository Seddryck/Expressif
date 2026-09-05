---
layout: docs
title: "any"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 10
has_toc: false
permalink: /accumulators/array/any/
tags:
  - accumulators
  - array
generated: true
---

```
any()
```

Returns `true` when at least one accumulated boolean value is `true`.



## Parameters



This accumulator has no parameters.






## Examples

{% raw %}
```expressif
{#false, #true, #false} | fold(any) → #true
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `any`
{: .member-reference }
