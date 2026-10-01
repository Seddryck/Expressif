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
grouping →
map-groups(
    expression: expression
) → grouping
```

Transforms each group's value collection while preserving its key and position.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | The expression evaluated once against each group's value collection. |



## Value shape

- **Pipeline input:** `grouping<K, T>`
- **Returns:** `grouping<K, U>`
- **`expression`:** Receives `array<T>` and returns `array<U>`.
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- **Cardinality:** `preserved`
- **Dependency:** `per-element`
- **Ordering:** `preserved`

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
