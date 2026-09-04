---
layout: docs
title: "text-to-uri"
parent: "Encoding functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/encoding/text-to-uri/
tags:
  - functions
  - text/encoding
generated: true
---

```
text →
text-to-uri() → text
```

Returns the input text escaped as URI data using UTF-8 percent encoding. Preserves `null`, empty, and blank inputs.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"café & tea" | text-to-uri → "caf%C3%A9%20%26%20tea"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/encoding`  
**Aliases:** None
{: .member-reference }
