---
layout: docs
title: "null-to-zero"
parent: "Conversion functions"
grand_parent: "Numeric functions"
nav_order: 10
has_toc: false
permalink: /functions/numeric/conversion/null-to-zero/
tags:
  - functions
  - numeric/conversion
generated: true
---

```
numeric →
null-to-zero() → numeric
```

Returns the unmodified argument value except if the argument value is `null`, `empty` or `whitespace` then it returns `0`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
10 | null-to-zero → 10
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/conversion`  
**Aliases:** None
{: .member-reference }
