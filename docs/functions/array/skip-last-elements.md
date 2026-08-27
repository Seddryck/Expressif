---
layout: docs
title: "skip-last-elements"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 160
has_toc: false
permalink: /functions/array/skip-last-elements/
tags:
  - functions
  - array
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





## Examples

{% raw %}
```expressif
{1, 2, 3} | skip-last-elements(2) → {1}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `skip-last`
{: .member-reference }
