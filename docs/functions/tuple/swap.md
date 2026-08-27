---
layout: docs
title: "swap"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/tuple/swap/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
swap(
    first: integer,
    second: integer
) → tuple
```

Returns a tuple with two positions exchanged, defaulting to the first and last positions.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `first` | `integer` | Yes | Specifies the first zero-based position. |
| `second` | `integer` | Yes | Specifies the second zero-based position. |






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
