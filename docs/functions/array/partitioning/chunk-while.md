---
layout: docs
title: "chunk-while"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/partitioning/chunk-while/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
chunk-while(
    operation: expression
) → array
```

Groups consecutive values while an operation over each previous and current pair evaluates to `true`. Returns `null` when the operation does not produce a Boolean value or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the callable or open expression that decides whether the current value continues the preceding chunk. |





## Behavior

`chunk-while` starts a chunk with the first input value. For every subsequent value, it evaluates the operation using the same consecutive-pair convention as `adjacent`: the previous value becomes the operation input and the current value supplies its missing argument. A `true` result appends the current value to the active chunk; `false` starts a new chunk. The operation is not evaluated for an empty or singleton input.



## Examples

{% raw %}
```expressif
{10, 20, 21, 22, 30, 31} | chunk-while(subtract | is-less-than(2)) → {{10}, {20, 21, 22}, {30, 31}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
