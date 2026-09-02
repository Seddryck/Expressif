---
layout: docs
title: "code-point"
parent: "Conversion functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/conversion/code-point/
tags:
  - functions
  - text/conversion
generated: true
---

```
text →
code-point() → integer
```

Returns the Unicode code point represented by a single Unicode scalar value. Returns `null` for any other input.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"😀" | code-point → 128512
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/conversion`  
**Aliases:** None
{: .member-reference }
