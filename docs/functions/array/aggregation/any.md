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



## Value shape

- **Pipeline input:** `boolean`
- **Returns:** `boolean`





## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.





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
