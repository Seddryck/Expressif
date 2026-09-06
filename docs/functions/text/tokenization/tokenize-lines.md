---
layout: docs
title: "tokenize-lines"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 80
has_toc: false
permalink: /functions/text/tokenization/tokenize-lines/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-lines() → array
```

Returns lines in source order, recognizing CR, LF, and CRLF as separators. Preserves spaces and empty lines, including a final empty line after a trailing separator. Returns an empty array for null or empty input.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"first" | suffix-new-line | suffix("second") | tokenize-lines → {"first", "second"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** None
{: .member-reference }
