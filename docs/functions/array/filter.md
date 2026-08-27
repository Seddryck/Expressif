---
layout: docs
title: "filter"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/array/filter/
tags:
  - functions
  - array
generated: true
---

```
array →
filter(
    predicate: predicate
) → array
```

Applies a predicate expression to each input item and returns only items for which the predicate evaluates to `true`. Returns `null` when the input is not an enumerable or is a string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicate` | `predicate` | Yes | Expression defining the predicate applied to each input item. |






## Examples

{% raw %}
```expressif
{1, 2, 3} | filter(greater-than(1)) → {2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `filter`
{: .member-reference }
