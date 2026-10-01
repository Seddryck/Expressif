---
layout: docs
title: "concat"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/aggregation/concat/
tags:
  - functions
  - array/aggregation
generated: true
---

```
text →
concat(
    separator: text = ""
) → text
```

Combines accumulated text values in source order, inserting the separator only between values.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `separator` | `text` | No | Specifies the text inserted between consecutive accumulated values. Defaults to `""`. |



## Examples

{% raw %}
```expressif
{"a", "b", "c"} | concat("-") → "a-b-c"
```
{% endraw %}

## Value shape

- Pipeline input: `text`
- Returns: `text`





## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.


## Argument evaluation

- **`separator`:** Evaluated once in the enclosing context before accumulation. The result is reused between accumulated values.


## Behavior

The separator defaults to the empty string. Empty input returns empty text. Empty values still participate in separator placement, and accumulating `null` is invalid.

**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
