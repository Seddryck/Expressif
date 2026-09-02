---
layout: docs
title: "single"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/selection/single/
tags:
  - functions
  - array/selection
generated: true
---

```
array →
single() → any
```

Returns the only element of the input array without transforming it. Returns `null` when the input is empty, contains more than one element, or cannot be evaluated as an array.

## Parameters



This function has no parameters.





## Behavior

`single` expresses an exact-cardinality requirement: the input must contain exactly one element. A sole `null` value is still the only element and therefore returns `null`; scalar and structured values retain their runtime type and value. Unlike `first-elements(1)`, `single` returns an element rather than an array and rejects additional elements by returning `null`.



## Examples

{% raw %}
```expressif
{42} | single → 42
{} | single → #null
{1, 2} | single → #null
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** None
{: .member-reference }
