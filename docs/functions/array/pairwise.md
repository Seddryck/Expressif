---
layout: docs
title: "pairwise"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 120
has_toc: false
permalink: /functions/array/pairwise/
tags:
  - functions
  - array
generated: true
---

```
array →
pairwise() → array
```

Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
{1, 2, 3} | pairwise → {T(1, 2), T(2, 3)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `pairwise`
{: .member-reference }
