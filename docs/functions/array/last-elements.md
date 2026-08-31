---
layout: docs
title: "last-elements"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/array/last-elements/
tags:
  - functions
  - array
generated: true
---

```
array →
last-elements(
    count: integer
) → array
```

Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to return from the end of the input. |






## Behavior

When `count` is greater than the number of elements in the input, `last-elements` returns all available elements in their original order. It does not pad the result to reach the requested count.



## Examples

{% raw %}
```expressif
{1, 2, 3} | last-elements(2) → {2, 3}
{1, 2, 3} | last-elements(5) → {1, 2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `last`
{: .member-reference }
