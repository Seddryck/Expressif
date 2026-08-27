---
layout: docs
title: "replace-slice"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 100
has_toc: false
permalink: /functions/text/concatenation/replace-slice/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
replace-slice(
    start: integer,
    length: integer,
    append: text
) → text
```

Returns the argument value with a subset of the string substitued by a another string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `start` | `integer` | Yes | The position to start to replace |
| `length` | `integer` | Yes | The length to replace |
| `append` | `text` | Yes | The text to append when the slice has been removed |





## Examples

{% raw %}
```expressif
"Hello World" | replace-slice(1, 2, "X") → "HXlo World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-replace-slice`
{: .member-reference }
