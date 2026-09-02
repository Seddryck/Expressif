---
layout: docs
title: "slice-elements"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/selection/slice-elements/
tags:
  - functions
  - array/selection
generated: true
---

```
array →
slice-elements(
    start: integer,
    end: integer
) → array
```

Returns the elements in the zero-based half-open range from start, inclusive, to end, exclusive. Returns `null` when the input is not an enumerable, is a string, or either bound is negative.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `start` | `integer` | Yes | Zero-based index of the first element to return. |
| `end` | `integer` | Yes | Zero-based exclusive index at which to stop returning elements. |





## Behavior

When `start` is greater than `end`, the requested range contains no elements and `slice-elements` returns an empty array.



## Examples

{% raw %}
```expressif
{1, 2, 3} | slice-elements(1, 3) → {2, 3}
{1, 2, 3} | slice-elements(2, 1) → {}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `slice`
{: .member-reference }
