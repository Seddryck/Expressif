---
layout: docs
title: "count"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 70
has_toc: false
permalink: /functions/array/aggregation/count/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
count() → any
```

Counts the number of accumulated items, including `null` values.



## Parameters



This function has no parameters.








## Examples

{% raw %}
```expressif
{10, #null, 30} | fold(count) → 3
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
