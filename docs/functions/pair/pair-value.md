---
layout: docs
title: "pair-value"
parent: "Pair functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/pair/pair-value/
tags:
  - functions
  - pair
generated: true
---

```
pair →
pair-value() → any
```

Returns the value component of the input pair.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `pair<K, V>`
- **Returns:** `V`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.







## Examples

{% raw %}
```expressif
("BE" => 42) | pair-value → 42
```
{% endraw %}


**Kind:** Function  
**Scope:** `pair`  
**Aliases:** None
{: .member-reference }
