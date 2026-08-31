---
layout: docs
title: "skip-first-elements"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 160
has_toc: false
permalink: /functions/array/skip-first-elements/
tags:
  - functions
  - array
generated: true
---

```
array →
skip-first-elements(
    count: integer
) → array
```

Omits the requested number of elements from the start of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to omit from the start of the input. |






## Behavior

When `count` is greater than the number of elements in the input, `skip-first-elements` omits all available elements and returns an empty array. Additional requested skips have no effect.



## Examples

{% raw %}
```expressif
{1, 2, 3} | skip-first-elements(2) → {3}
{1, 2, 3} | skip-first-elements(5) → {}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `skip-first`
{: .member-reference }
