---
layout: docs
title: "pair-key"
parent: "Pair functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/pair/pair-key/
tags:
  - functions
  - pair
generated: true
---

```
pair →
pair-key() → any
```

Returns the key component of the input pair.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `pair<K, V>`
- **Returns:** `K`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.







## Examples

{% raw %}
```expressif
("BE" => 42) | pair-key → "BE"
```
{% endraw %}


**Kind:** Function  
**Scope:** `pair`  
**Aliases:** None
{: .member-reference }
