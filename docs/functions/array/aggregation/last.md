---
layout: docs
title: "last"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 110
has_toc: false
permalink: /functions/array/aggregation/last/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
last() → any
```

Stores the most recently accumulated item.



## Parameters



This function has no parameters.








## Examples

{% raw %}
```expressif
{10, 20, 30} | fold(last) → 30
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
