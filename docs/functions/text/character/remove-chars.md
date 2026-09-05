---
layout: docs
title: "remove-chars"
parent: "Character functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/character/remove-chars/
tags:
  - functions
  - text/character
generated: true
---

```
text →
remove-chars(
    charToRemove: text
) → text
```

Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `charToRemove` | `text` | Yes | The char to be removed from the argument string. |





## Examples

{% raw %}
```expressif
"Hello World" | remove-chars("l") → "Heo Word"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/character`  
**Aliases:** `text-to-remove-chars`
{: .member-reference }
