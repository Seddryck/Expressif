---
layout: docs
title: "distribute-round-robin"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 70
has_toc: false
permalink: /functions/array/partitioning/distribute-round-robin/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
distribute-round-robin(
    count: integer
) → array
```

Distributes successive array values cyclically among a requested number of output arrays. Returns `null` when the count is not strictly positive or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Specifies the strictly positive number of output arrays. |





## Behavior

The value at zero-based input position `i` is assigned to output array `i modulo count`. The result always contains exactly `count` arrays, including empty trailing arrays when the count exceeds the input cardinality. Relative input order is preserved within every output array. Empty input returns `count` empty arrays.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4, 5} | distribute-round-robin(2) → {{1, 3, 5}, {2, 4}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
