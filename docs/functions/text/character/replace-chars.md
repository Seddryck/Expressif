---
layout: docs
title: "replace-chars"
parent: "Character functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/character/replace-chars/
tags:
  - functions
  - text/character
generated: true
---

```
text →
replace-chars(
    charToReplace: text,
    charReplacing: text
) → text
```

Returns the argument value where a specific char has been replaced by another, both specified as parameters.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `charToReplace` | `text` | Yes | The char to be replaced from the argument string. |
| `charReplacing` | `text` | Yes | The replacing char from the argument string. |





## Examples

{% raw %}
```expressif
"Hello World" | replace-chars("l", "x") → "Hexxo Worxd"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/character`  
**Aliases:** `text-to-replace-chars`
{: .member-reference }
