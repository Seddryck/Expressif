---
layout: docs
title: "single"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/selection/single/
tags:
  - functions
  - array/selection
generated: true
---

```
array<T> →
single() → nullable<T>
```

Returns the only element of the input array without transforming it. Returns `null` when the input is empty, contains more than one element, or cannot be evaluated as an array.



## Parameters



This function has no parameters.



## Value shape

- Pipeline input: `array<T>`
- Returns: `nullable<T>`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `collapsed` <span class="semantics-info" title="The visited collection produces one result." aria-label="Cardinality definition: The visited collection produces one result.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `not-applicable` <span class="semantics-info" title="The result has no element ordering to describe." aria-label="Ordering definition: The result has no element ordering to describe.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.




## Behavior

`single` expresses an exact-cardinality requirement: the input must contain exactly one element. A sole `null` value is still the only element and therefore returns `null`; scalar and structured values retain their runtime type and value. Unlike `first-elements(1)`, `single` returns an element rather than an array and rejects additional elements by returning `null`.



## Examples

{% raw %}
```expressif
{42} | single → 42
{} | single → #null
{1, 2} | single → #null
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** None
{: .member-reference }
