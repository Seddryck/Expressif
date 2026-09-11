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



## Argument evaluation

Visits consecutive elements of the array supplied as pipeline input to this chunk-while call, in source order, starting with the second element.

- **`operation`:** Evaluated once per consecutive pair, with T(previous, current) as its input and argument context; $0 resolves to the previous element and $1 to the current element. Empty and singleton arrays do not evaluate the operation.


## Behavior

The operation receives T(previous, current). `~f` invokes current | f(previous), while `f~` invokes previous | f(current); subsequent stages keep that pair as their argument context. A Boolean result of true continues the current chunk, false starts another, and a non-Boolean result returns null. Legacy implicit argument injection is deprecated but preserved: a leading bare callable in a composed operation still invokes current | f(previous). Use an explicit binding or `$1 | f($0)`; the operator and callable themselves are not deprecated.



## Examples

{% raw %}
```expressif
{10, 20, 21, 22, 30, 31} | chunk-while(~subtract | is-less-than(2)) → {{10}, {20, 21, 22}, {30, 31}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
