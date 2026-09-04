---
layout: docs
title: "text-to-json-escaped"
parent: "Encoding functions"
grand_parent: "Text functions"
nav_order: 40
has_toc: false
permalink: /functions/text/encoding/text-to-json-escaped/
tags:
  - functions
  - text/encoding
generated: true
---

```
text →
text-to-json-escaped() → text
```

Returns the escaped contents of a JSON string without surrounding quotation marks. Preserves `null`, empty, and blank inputs.

## Parameters



This function has no parameters.





## Behavior

Escapes quotation marks, reverse solidus characters, and control characters required by the JSON string grammar while leaving valid Unicode text readable. Already escaped content is escaped again.



## Examples

{% raw %}
```expressif
"He said \"hello\"" | text-to-json-escaped → "He said \\\"hello\\\""
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/encoding`  
**Aliases:** None
{: .member-reference }
