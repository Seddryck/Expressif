---
layout: docs
title: "text-to-xml-escaped"
parent: "Encoding functions"
grand_parent: "Text functions"
nav_order: 60
has_toc: false
permalink: /functions/text/encoding/text-to-xml-escaped/
tags:
  - functions
  - text/encoding
generated: true
---

```
text →
text-to-xml-escaped() → text
```

Returns text escaped for use as XML character data without adding a containing element. Returns `null` for characters that are invalid in XML and preserves `null`, empty, and blank inputs.

## Parameters



This function has no parameters.





## Behavior

Escapes ampersands and angle brackets for XML text-node content. Quotation marks remain unchanged because the result is character data rather than an attribute value. Already escaped content is escaped again.



## Examples

{% raw %}
```expressif
"A & B < C" | text-to-xml-escaped → "A &amp; B &lt; C"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/encoding`  
**Aliases:** None
{: .member-reference }
