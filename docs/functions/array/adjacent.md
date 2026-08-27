---
layout: docs
title: "adjacent"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/array/adjacent/
tags:
  - functions
  - array
generated: true
---

```
array →
adjacent(
    operation: expression
) → array
```

Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the callable or open expression evaluated against each consecutive pair. |





## Behavior

`adjacent` evaluates the supplied operation against each consecutive pair of values.

For each pair, the first value becomes the input of the operation and the second value is supplied as its missing argument. This means that a binary function such as `subtract` can be passed directly.

For example, `{1, 2, 3} | adjacent(subtract)` is conceptually equivalent to applying `$1 | subtract($0)` to each consecutive pair.

## Examples

{% raw %}
```expressif
{1, 2, 3} | adjacent(subtract) → {1, 1}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `adjacent`
{: .member-reference }
