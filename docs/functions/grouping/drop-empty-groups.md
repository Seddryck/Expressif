---
layout: docs
title: "drop-empty-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/grouping/drop-empty-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping<K, T> →
drop-empty-groups() → grouping<K, T>
```

Removes groups whose value collection contains no items.



## Parameters



This function has no parameters.



## Value shape

- Pipeline input: `grouping<K, T>`
- Returns: `grouping<K, T>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.





## Examples

{% raw %}
```expressif
#{("BE" => {}), ("FR" => {15})} | drop-empty-groups → #{("FR" => {15})}
#{("BE" => {#null})} | drop-empty-groups → #{("BE" => {#null})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
