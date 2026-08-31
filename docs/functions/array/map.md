---
layout: docs
title: "map"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/array/map/
tags:
  - functions
  - array
generated: true
---

```
array →
map(
    transformation: expression
) → array
```

Applies a transformation expression to each input item and returns the transformed values. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `transformation` | `expression` | Yes | Expression creating the transformation applied to each input item. |






## Examples

{% raw %}
```expressif
{1, 2, 3} | map(add(1)) → {2, 3, 4}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `map`
{: .member-reference }
