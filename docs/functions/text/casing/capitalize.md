---
layout: docs
title: "capitalize"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 40
has_toc: false
permalink: /functions/text/casing/capitalize/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
capitalize() → text
```

Returns the input text with its first word capitalized and the remaining content preserved. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `AT&T`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"hello World" | capitalize → "Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** None
{: .member-reference }
