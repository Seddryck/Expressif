---
layout: docs
title: "human-readable-format-binary-bytes"
parent: "Formatting functions"
grand_parent: "Numeric functions"
nav_order: 10
has_toc: false
permalink: /functions/numeric/formatting/human-readable-format-binary-bytes/
tags:
  - functions
  - numeric/formatting
generated: true
---

```
numeric →
human-readable-format-binary-bytes() → text
```

Formats a numeric value as binary bytes using IEC prefixes.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
10 | human-readable-format-binary-bytes → "10 B"
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/formatting`  
**Aliases:** `numeric-to-human-readable-format-binary-bytes`
{: .member-reference }
