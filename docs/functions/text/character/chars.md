---
layout: docs
title: "chars"
parent: "Character functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/character/chars/
tags:
  - functions
  - text/character
generated: true
---

```
text →
chars() → array
```

Returns the characters in the input text as an array in source order. Returns `null` for `null` and an empty array for empty text.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"123" | chars → {"1", "2", "3"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/character`  
**Aliases:** `chars`
{: .member-reference }
