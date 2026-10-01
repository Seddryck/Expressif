---
layout: docs
title: "first"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 90
has_toc: false
permalink: /functions/array/aggregation/first/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
first() → any
```

Stores the first accumulated item and ignores all subsequent items.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `T`
- **Returns:** `nullable<T>`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.




## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.





## Examples

{% raw %}
```expressif
{10, 20, 30} | fold(first) → 10
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
