---
layout: docs
title: "map-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/grouping/map-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping<K, T> →
map-groups(
    expression: expression
) → grouping<K, U>
```

Transforms each group's value collection while preserving its key and position.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | The expression evaluated once against each group's value collection. |



## Value shape

- Pipeline input: `grouping<K, T>`
- Returns: `grouping<K, U>`
- `expression`: Receives `array<T>` and returns `array<U>`.
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each group in the incoming grouping and supplies its entire value collection to the expression.

- **`expression`:** Evaluated once per group against that group's entire value collection.



## Examples

{% raw %}
```expressif
#{("BE" => {10, 20, 30}), ("FR" => {5, 15})} | map-groups(filter(greater-than(10))) → #{("BE" => {20, 30}), ("FR" => {15})}
#{("BE" => {10}), ("FR" => {20})} |#> filter(greater-than(15)) → #{("BE" => {}), ("FR" => {20})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
