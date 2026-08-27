---
layout: docs
title: "prefix"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 40
has_toc: false
permalink: /functions/text/concatenation/prefix/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prefix(
    prefix: text
) → text
```

Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns `null`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `prefix` | `text` | Yes | The text to append |





## Examples

{% raw %}
```expressif
"Hello World" | prefix("Hi ") → "Hi Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prefix`
{: .member-reference }
