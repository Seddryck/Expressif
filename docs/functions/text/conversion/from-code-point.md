---
layout: docs
title: "from-code-point"
parent: "Conversion functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/conversion/from-code-point/
tags:
  - functions
  - text/conversion
generated: true
---

```
numeric →
from-code-point() → text
```

Returns the text corresponding to an integer Unicode scalar value. Returns `null` for any other input.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
128512 | from-code-point → "😀"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/conversion`  
**Aliases:** None
{: .member-reference }
