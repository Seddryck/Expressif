---
layout: docs
title: "group"
parent: "Grouping functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/grouping/group/
tags:
  - functions
  - array/grouping
generated: true
---

```
array →
group() → grouping
```

Groups pairs by structurally equal keys while preserving first-seen group and value order.



## Parameters



This function has no parameters.



## Structural semantics

- **Cardinality:** `partitioned`
- **Dependency:** `partition`
- **Ordering:** `preserved`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.



## Examples

{% raw %}
```expressif
{("BE" => "Alice"), ("FR" => "Charlie"), ("BE" => "Bob")} | group → #{("BE" => {"Alice", "Bob"}), ("FR" => {"Charlie"})}
{} | group → #{}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/grouping`  
**Aliases:** None
{: .member-reference }
