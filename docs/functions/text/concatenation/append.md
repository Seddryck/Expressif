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


> **Deprecated:** Use `suffix` instead. This function is planned for removal in Expressif 3.0.


## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `text` | `text` | Yes | The text to append |





## Behavior

Deprecated in favor of `suffix` and planned for removal in Expressif 3.0. A direct replacement changes null handling because `suffix` preserves `null`. Use `null-to-empty | suffix(...)` to retain the existing behavior for null input.



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
