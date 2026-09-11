---
layout: docs
title: "swap"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/tuple/swap/
tags:
  - functions
  - tuple
generated: true
---

```
tuple | vector →
swap(
    first: integer,
    second: integer
) → tuple | vector
```

Returns a tuple with two positions exchanged, defaulting to the first and last positions.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `first` | `integer` | Yes | Specifies the first zero-based position. |
| `second` | `integer` | Yes | Specifies the second zero-based position. |



## Argument evaluation

- **`first`:** Evaluated once in the enclosing context.
- **`second`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
T("a", "b", "c", "d") | swap → T("d", "b", "c", "a")
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `swap`
{: .member-reference }
