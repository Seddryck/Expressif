---
layout: docs
title: "cardinality"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/array/cardinality/
tags:
  - functions
  - array
generated: true
---

```
array →
cardinality() → integer
```

Returns the number of elements in the input array.



## Parameters



This function has no parameters.



## Structural semantics

- **Cardinality:** `collapsed`
- **Dependency:** `whole-input`
- **Ordering:** `not-applicable`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.



## Examples

{% raw %}
```expressif
{} | cardinality → 0
{1, 2, 3} | cardinality → 3
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
