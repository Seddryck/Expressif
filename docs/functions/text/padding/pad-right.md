---
layout: docs
title: "pad-right"
parent: "Padding functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/padding/pad-right/
tags:
  - functions
  - text/padding
generated: true
---

```
text →
pad-right(
    length: integer,
    character: text
) → text
```

Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the minimal length of the string returned |
| `character` | `text` | Yes | The padding character |





## Examples

{% raw %}
```expressif
"Hello World" | pad-right(2, "-") → "Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/padding`  
**Aliases:** `text-to-pad-right`
{: .member-reference }
