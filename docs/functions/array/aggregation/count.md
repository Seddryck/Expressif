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
T →
count() → integer
```

Counts the number of accumulated items, including `null` values.



## Parameters



This function has no parameters.



## Value shape

- Pipeline input: `T`
- Returns: `integer`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.




## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.





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
