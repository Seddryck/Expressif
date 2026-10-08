---
layout: docs
title: "swap"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 80
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
    first?: integer,
    second?: integer
) → tuple | vector
```

Returns a tuple with two positions exchanged, defaulting to the first and last positions.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `first` | `integer` | No | Specifies the first zero-based position. Omission is preserved for operator-specific handling. |
| `second` | `integer` | No | Specifies the second zero-based position. Omission is preserved for operator-specific handling. |
## Examples

{% raw %}
```expressif
T("a", "b", "c", "d") | swap → T("d", "b", "c", "a")
```
{% endraw %}

## Argument evaluation

- **`first`:** Evaluated once in the context surrounding this `swap` call.
- **`second`:** Evaluated once in the context surrounding this `swap` call.

**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `swap`
{: .member-reference }
