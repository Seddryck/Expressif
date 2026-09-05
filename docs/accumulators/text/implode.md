---
layout: docs
title: "implode"
parent: "Text accumulators"
grand_parent: "Accumulators library"
nav_order: 10
has_toc: false
permalink: /accumulators/text/implode/
tags:
  - accumulators
  - text
generated: true
---

```
text →
implode(
    separator?: text
) → text
```

Combines accumulated text values in source order, inserting the separator only between values.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `separator` | `text` | No | Specifies the text inserted between consecutive accumulated values. |





## Behavior

The separator defaults to the empty string. Empty input returns empty text. Empty values still participate in separator placement, and accumulating `null` is invalid.



## Examples

{% raw %}
```expressif
{"a", "b", "c"} | implode("-") → "a-b-c"
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `text`  
**Aliases:** `implode`
{: .member-reference }
