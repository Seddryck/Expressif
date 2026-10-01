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
T →
last() → nullable<T>
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

## Value shape

- Pipeline input: `T`
- Returns: `nullable<T>`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.




## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.

**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
