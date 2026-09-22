---
layout: docs
title: "count"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 50
has_toc: false
permalink: /accumulators/array/count/
tags:
  - accumulators
  - array
generated: true
---

```
array →
count() → any
```

Counts the number of accumulated items, including `null` values.



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
{10, #null, 30} | fold(count) → 3
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
