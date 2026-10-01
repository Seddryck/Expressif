---
layout: docs
title: "fold"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 100
has_toc: false
permalink: /functions/array/aggregation/fold/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array<T> →
fold(
    accumulator: accumulator
) → U
```

Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `accumulator` | `accumulator` | Yes | Factory that creates the accumulator instance used for the fold execution. |



## Value shape

- Pipeline input: `array<T>`
- Returns: `U`
- `accumulator`: Receives `T` and returns `U`.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `collapsed` <span class="semantics-info" title="The visited collection produces one result." aria-label="Cardinality definition: The visited collection produces one result.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `not-applicable` <span class="semantics-info" title="The result has no element ordering to describe." aria-label="Ordering definition: The result has no element ordering to describe.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array entering this call.

- **`accumulator`:** The selected accumulator receives each incoming array element through its accumulation lifecycle; the accumulator factory is not recreated for each element.



## Examples

{% raw %}
```expressif
{1, 2, 3} | fold(sum) → 6
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** `array-to-fold`
{: .member-reference }
