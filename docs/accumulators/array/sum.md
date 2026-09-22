---
layout: docs
title: "sum"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 130
has_toc: false
permalink: /accumulators/array/sum/
tags:
  - accumulators
  - array
generated: true
---

```
array →
sum() → any
```

Computes the sum of all accumulated numeric values.



## Parameters



This accumulator has no parameters.



## Structural semantics

- **Cardinality:** `collapsed`
- **Dependency:** `whole-input`
- **Ordering:** `not-applicable`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.



## Examples

{% raw %}
```expressif
{10, 20, 30} | fold(sum) → 60
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
