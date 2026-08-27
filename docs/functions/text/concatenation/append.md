---
layout: docs
title: "append"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/concatenation/append/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
append(
    text: text
) → text
```

Returns the argument value followed by the parameter value. If the argument is `null`, it returns the text specified as the parameter.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `text` | `text` | Yes | The text to append |





## Examples

{% raw %}
```expressif
"Hello World" | append("Hi ") → "Hello WorldHi "
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-append`
{: .member-reference }
