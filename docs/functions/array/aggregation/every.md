---
layout: docs
title: "every"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 80
has_toc: false
permalink: /functions/array/aggregation/every/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
every() → any
```

Returns `true` only when every accumulated boolean value is `true`.



## Parameters



This function has no parameters.








## Examples

{% raw %}
```expressif
{#true, #true, #false} | fold(every) → #false
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
