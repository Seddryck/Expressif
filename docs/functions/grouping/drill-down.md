---
layout: docs
title: "drill-down"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/grouping/drill-down/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
drill-down(
    ...expressions: expression
) → grouping
```

Refines each existing group by appending dimensions derived from its values.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expressions` | `expression` | Variadic (one or more) | One or more expressions whose results are appended to the existing key. |



## Argument evaluation

Visits each value within each group of the grouping supplied as pipeline input to this drill-down call, in parent-group and value order.

- **`expressions`:** Each supplied expression is evaluated once per grouped value, in declaration order, with that value as its context; .field reads that value's field.


## Behavior

Appends to an existing tuple key or starts a tuple with an existing scalar key. Derived tuples remain single dimensions. Structural equality includes null; parent groups, first-seen subgroups and values retain their order. Empty groups produce no subgroups. Like group-by, requires one or more positional expressions; named arguments and spread are rejected.



## Examples

{% raw %}
```expressif
#{("BE" => {1, 2})} | drill-down(@_) → #{(T("BE", 1) => {1}), (T("BE", 2) => {2})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
