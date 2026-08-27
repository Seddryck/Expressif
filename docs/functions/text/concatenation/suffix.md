---
layout: docs
title: "suffix"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 110
has_toc: false
permalink: /functions/text/concatenation/suffix/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
suffix(
    suffix: text
) → text
```

Returns the argument value followed by the parameter value. If the argument is `null`, it returns `null`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `suffix` | `text` | Yes | The text to append |





## Examples

{% raw %}
```expressif
"Hello World" | suffix("!") → "Hello World!"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-suffix`
{: .member-reference }
