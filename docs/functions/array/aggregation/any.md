---
layout: docs
title: "any"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/aggregation/any/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
any() → any
```

Returns `true` when at least one accumulated boolean value is `true`.



## Parameters



This function has no parameters.








## Examples

{% raw %}
```expressif
{#false, #true, #false} | fold(any) → #true
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
