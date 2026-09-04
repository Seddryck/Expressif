---
layout: docs
title: "uri-to-text"
parent: "Encoding functions"
grand_parent: "Text functions"
nav_order: 70
has_toc: false
permalink: /functions/text/encoding/uri-to-text/
tags:
  - functions
  - text/encoding
generated: true
---

```
text →
uri-to-text() → text
```

Returns text by unescaping one layer of URI percent encoding. Preserves `null`, empty, and blank inputs.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"caf%C3%A9%20%26%20tea" | uri-to-text → "café & tea"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/encoding`  
**Aliases:** None
{: .member-reference }
