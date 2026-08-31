---
layout: docs
title: "tokenize-snake"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 90
has_toc: false
permalink: /functions/text/tokenization/tokenize-snake/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-snake() → array
```

Returns normalized tokens from an underscore-separated name, preserving escaped underscores within tokens.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"first_name" | tokenize-snake → {"first", "name"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-tokenize-snake`
{: .member-reference }
