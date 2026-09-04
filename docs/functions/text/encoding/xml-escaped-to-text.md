---
layout: docs
title: "xml-escaped-to-text"
parent: "Encoding functions"
grand_parent: "Text functions"
nav_order: 80
has_toc: false
permalink: /functions/text/encoding/xml-escaped-to-text/
tags:
  - functions
  - text/encoding
generated: true
---

```
text →
xml-escaped-to-text() → text
```

Returns text by decoding XML character data without requiring a containing element. Returns `null` for malformed input and preserves `null`, empty, and blank inputs.

## Parameters



This function has no parameters.





## Behavior

Decodes one layer of the five predefined XML entities and valid decimal or hexadecimal numeric character references. Markup, document type declarations, unknown entities, and invalid XML characters are rejected.



## Examples

{% raw %}
```expressif
"A &amp; B &lt; C" | xml-escaped-to-text → "A & B < C"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/encoding`  
**Aliases:** None
{: .member-reference }
