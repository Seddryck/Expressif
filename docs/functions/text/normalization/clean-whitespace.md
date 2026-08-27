---
layout: docs
title: "clean-whitespace"
parent: "Normalization functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/normalization/clean-whitespace/
tags:
  - functions
  - text/normalization
generated: true
---

```
text →
clean-whitespace() → text
```

returns the argument with any whitespace replaced by a space character. `\r\n` is considered as a single character.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | clean-whitespace → "Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/normalization`  
**Aliases:** `text-to-clean-whitespace`
{: .member-reference }
