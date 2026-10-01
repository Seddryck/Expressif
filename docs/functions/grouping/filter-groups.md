---
layout: docs
title: "filter-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/grouping/filter-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping<K, T> →
filter-groups(
    predicate: predicate
) → grouping<K, T>
```

Keeps whole groups whose group-level predicate evaluates to true.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicate` | `predicate` | Yes | The predicate evaluated once against each group, with its key and value collection available. |



## Value shape

- Pipeline input: `grouping<K, T>`
- Returns: `grouping<K, T>`
- `predicate`: Receives `pair<K, array<T>>` and returns `boolean`.
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each group, including its key and values, in the grouping entering this call.

- **`predicate`:** Evaluated once per visited group, with its key and values available in the context.



## Examples

{% raw %}
```expressif
#{("BE" => {1, 2}), ("FR" => {3})} | filter-groups($value | cardinality | greater-than(1)) → #{("BE" => {1, 2})}
#{("BE" => {1}), ("FR" => {2})} | having($key | is-equivalent-to("FR")) → #{("FR" => {2})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** `having`
{: .member-reference }
