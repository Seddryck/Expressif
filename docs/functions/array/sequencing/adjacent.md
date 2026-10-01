---
layout: docs
title: "adjacent"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/sequencing/adjacent/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array<T> →
adjacent(
    operation: expression
) → array<U>
```

Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the callable or open expression evaluated against each consecutive pair. |



## Examples

{% raw %}
```expressif
{1, 2, 3} | adjacent(~subtract) → {1, 1}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<U>`
- `operation`: Receives `tuple<T, T>` and returns `U`.
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `prefix` <span class="semantics-info" title="An output at a position depends on the visited prefix ending at that position." aria-label="Dependency definition: An output at a position depends on the visited prefix ending at that position.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits consecutive elements of the array supplied as pipeline input to this adjacent call, in source order, starting with the second element.

- **`operation`:** Evaluated once for each candidate after the first element, with T(currentChunk, candidate) as its input and argument context; $0 resolves to a stable array of elements already accepted into this chunk, excluding the candidate, and $1 to the candidate. Empty and singleton arrays do not evaluate the operation.


## Behavior

The operation receives T(previous, current). `~f` invokes current | f(previous), while `f~` invokes previous | f(current); subsequent stages keep that pair as their argument context. Legacy implicit argument injection is deprecated but preserved: a single bare callable still invokes current | f(previous). Use an explicit binding or `$1 | f($0)`; the operator and callable themselves are not deprecated.

**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `adjacent`
{: .member-reference }
