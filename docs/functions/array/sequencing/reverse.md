---
layout: docs
title: "reverse"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/sequencing/reverse/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array →
reverse() → array
```

Returns the input enumerable with elements emitted in the opposite order. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<T>`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- **Cardinality:** `preserved`
- **Dependency:** `whole-input`
- **Ordering:** `reordered`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.





## Examples

{% raw %}
```expressif
{1, 2, 3} | reverse → {3, 2, 1}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `reverse`
{: .member-reference }
