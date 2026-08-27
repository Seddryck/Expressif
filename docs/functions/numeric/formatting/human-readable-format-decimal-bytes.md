---
layout: docs
title: "human-readable-format-decimal-bytes"
parent: "Formatting functions"
grand_parent: "Numeric functions"
nav_order: 30
has_toc: false
permalink: /functions/numeric/formatting/human-readable-format-decimal-bytes/
tags:
  - functions
  - numeric/formatting
generated: true
---

```
numeric →
human-readable-format-decimal-bytes() → text
```

Formats a numeric value as decimal bytes using SI prefixes.

## Parameters



This function has no parameters.





## Examples

```expressif
10 | human-readable-format-decimal-bytes → "10 B"
```


**Kind:** Function  
**Scope:** `numeric/formatting`  
**Aliases:** `numeric-to-human-readable-format-decimal-bytes`
{: .member-reference }
