---
layout: docs
title: "rotate"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/tuple/rotate/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
rotate(
    offset: integer = 1
) → tuple
```

Returns a tuple with its positions rotated cyclically, preserving arity and item values, including nulls.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `offset` | `integer` | No | Specifies the rotation offset: positive values rotate right and negative values rotate left, wrapping modulo tuple length. Defaults to 1; zero leaves the order unchanged. Defaults to `1`. |



## Argument evaluation

- **`offset`:** Evaluated once in the enclosing expression's context for each tuple supplied to this rotate call, including empty and single-item tuples. Field references read the enclosing record, and $0 and $1 read its first and second positions when that context is a tuple.


## Behavior

Empty and single-item tuples retain their values. Non-positional input returns null; offsets follow the standard integer argument conversion and validation rules, including conversion of null to zero. Unconvertible or out-of-range offsets are rejected.



## Examples

{% raw %}
```expressif
T(10, 20, 30) | rotate → T(30, 10, 20)
T(10, 20, 30) | rotate(1) → T(30, 10, 20)
T(10, 20, 30) | rotate(4) → T(30, 10, 20)
{items := T(10, 20, 30), offset := 1} | .items | rotate(.offset) → T(30, 10, 20)
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** None
{: .member-reference }
