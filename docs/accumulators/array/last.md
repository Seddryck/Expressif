---
layout: docs
title: "last"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 80
has_toc: false
permalink: /accumulators/array/last/
tags:
  - accumulators
  - array
generated: true
---

```
array →
last() → any
```

Stores the most recently accumulated item.



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
{10, 20, 30} | fold(last) → 30
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
