---
layout: docs
title: "slice-elements"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 180
has_toc: false
permalink: /functions/array/slice-elements/
tags:
  - functions
  - array
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






## Examples

{% raw %}
```expressif
{1, 2, 3} | slice-elements(1, 3) → {2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `slice`
{: .member-reference }
