---
layout: docs
title: "first-elements"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/selection/first-elements/
tags:
  - functions
  - array/selection
generated: true
---

```
array →
first-elements(
    count: integer
) → array
```

Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to return from the start of the input. |





## Behavior

When `count` is greater than the number of elements in the input, `first-elements` returns all available elements in their original order. It does not pad the result to reach the requested count.



## Examples

{% raw %}
```expressif
{1, 2, 3} | first-elements(2) → {1, 2}
{1, 2, 3} | first-elements(5) → {1, 2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `first`
{: .member-reference }
