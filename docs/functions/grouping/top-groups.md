---
layout: docs
title: "top-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 110
has_toc: false
permalink: /functions/grouping/top-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
top-groups(
    count: integer,
    expression: expression
) → grouping
```

Keeps up to count complete groups in descending ranking order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The maximum number of groups to select. |
| `expression` | `expression` | Yes | The expression that supplies each group's ranking score. |



## Argument evaluation

Visits each group of the grouping supplied as pipeline input to this top-groups call, including empty groups, when count is positive.

- **`count`:** Evaluated once in the surrounding context before any ranking expressions.
- **`expression`:** For a positive count, evaluated once per group in source order with that Group as context: $key reads its existing key and $value reads its complete value collection. It is not evaluated for zero count.


## Behavior

Ranks comparable scalar scores using the same numeric normalization and ordinal text ordering as min-by and max-by. Null scores sort last, as in normal descending sorting. Equal scores retain source-group order. Zero count returns an empty grouping without evaluating scores; negative counts fail with an argument error. Oversized counts return all groups in ranking order. Values are preserved without summarization.



## Examples

{% raw %}
```expressif
#{("BE" => {1}), ("FR" => {2, 3})} | top-groups(1, $value | cardinality) → #{("FR" => {2, 3})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
