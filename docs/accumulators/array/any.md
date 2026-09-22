---
layout: docs
title: "any"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 10
has_toc: false
permalink: /accumulators/array/any/
tags:
  - accumulators
  - array
generated: true
---

```
array →
any() → any
```

Returns `true` when at least one accumulated boolean value is `true`.



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
{#false, #true, #false} | fold(any) → #true
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
