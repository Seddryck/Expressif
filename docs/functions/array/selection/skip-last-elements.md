---
layout: docs
title: "skip-last-elements"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/selection/skip-last-elements/
tags:
  - functions
  - array/selection
generated: true
---

```
array →
skip-last-elements(
    count: integer
) → array
```

Omits the requested number of elements from the end of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to omit from the end of the input. |





## Behavior

When `count` is greater than the number of elements in the input, `skip-last-elements` omits all available elements and returns an empty array. Additional requested skips have no effect.



## Examples

{% raw %}
```expressif
{1, 2, 3} | skip-last-elements(2) → {1}
{1, 2, 3} | skip-last-elements(5) → {}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `skip-last`
{: .member-reference }
