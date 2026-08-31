---
layout: docs
title: "tokenize-kebab"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 60
has_toc: false
permalink: /functions/text/tokenization/tokenize-kebab/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-kebab() → array
```

Returns normalized tokens from a hyphen-separated name, preserving escaped hyphens within tokens.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"first-name" | tokenize-kebab → {"first", "name"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-tokenize-kebab`
{: .member-reference }
