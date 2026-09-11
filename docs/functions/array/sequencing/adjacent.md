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
array →
adjacent(
    operation: expression
) → array
```

Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the callable or open expression evaluated against each consecutive pair. |



## Argument evaluation

Visits consecutive elements of the array supplied as pipeline input to this adjacent call, in source order, starting with the second element.

- **`operation`:** Evaluated once per consecutive pair, with T(previous, current) as its input and argument context; $0 resolves to the previous element and $1 to the current element. Empty and singleton arrays do not evaluate the operation.


## Behavior

The operation receives T(previous, current). `~f` invokes current | f(previous), while `f~` invokes previous | f(current); subsequent stages keep that pair as their argument context. Legacy implicit argument injection is deprecated but preserved: a single bare callable still invokes current | f(previous). Use an explicit binding or `$1 | f($0)`; the operator and callable themselves are not deprecated.



## Examples

{% raw %}
```expressif
{1, 2, 3} | adjacent(~subtract) → {1, 1}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `adjacent`
{: .member-reference }
