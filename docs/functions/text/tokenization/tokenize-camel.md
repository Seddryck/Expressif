---
layout: docs
title: "tokenize-camel"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/tokenization/tokenize-camel/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-camel() → array
```

Returns tokens from a camelCase name using case and acronym transitions as boundaries.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"firstName" | tokenize-camel → {"first", "Name"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-tokenize-camel`
{: .member-reference }
