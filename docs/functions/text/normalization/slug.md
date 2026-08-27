---
layout: docs
title: "slug"
parent: "Normalization functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/normalization/slug/
tags:
  - functions
  - text/normalization
generated: true
---

```
text →
slug() → text
```

Returns a lowercase, separator-normalized slug, removing Latin diacritics without transliterating non-Latin scripts. Returns empty text when the input is `null`, empty, or blank.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Crème brûlée recipe" | slug → "creme-brulee-recipe"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/normalization`  
**Aliases:** `text-to-slug`
{: .member-reference }
