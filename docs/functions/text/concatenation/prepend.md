---
layout: docs
title: "prepend"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 70
has_toc: false
permalink: /functions/text/concatenation/prepend/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prepend(
    text: text
) → text
```

Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns the text specified as the parameter.


> **Deprecated:** Use `prefix` instead. This function is planned for removal in Expressif 3.0.


## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `text` | `text` | Yes | The text to prepend |





## Behavior

Deprecated in favor of `prefix` and planned for removal in Expressif 3.0. A direct replacement changes null handling because `prefix` preserves `null`. Use `null-to-empty | prefix(...)` to retain the existing behavior for null input.



## Examples

{% raw %}
```expressif
"Hello World" | prepend("Hi ") → "Hi Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prepend`
{: .member-reference }
