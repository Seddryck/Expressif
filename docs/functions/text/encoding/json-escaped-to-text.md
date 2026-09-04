---
layout: docs
title: "json-escaped-to-text"
parent: "Encoding functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/encoding/json-escaped-to-text/
tags:
  - functions
  - text/encoding
generated: true
---

```
text →
json-escaped-to-text() → text
```

Returns text by decoding escaped JSON string contents without requiring surrounding quotation marks. Returns `null` for malformed input and preserves `null`, empty, and blank inputs.

## Parameters



This function has no parameters.





## Behavior

Decodes one layer of JSON string escaping, including control-character escapes and valid Unicode escape sequences. The input is fragment content without surrounding JSON quotation marks.



## Examples

{% raw %}
```expressif
"He said \"hello\"" | json-escaped-to-text → "He said \"hello\""
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/encoding`  
**Aliases:** None
{: .member-reference }
