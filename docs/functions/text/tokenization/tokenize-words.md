---
layout: docs
title: "tokenize-words"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 100
has_toc: false
permalink: /functions/text/tokenization/tokenize-words/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-words() → array
```

Returns word tokens using separators, punctuation, symbols, case transitions, and acronym transitions as boundaries.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"customer_HTTP-server_id" | tokenize-words → {"customer", "HTTP", "server", "id"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-tokenize-words`
{: .member-reference }
