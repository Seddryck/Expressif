---
layout: docs
title: "retain-alpha-numeric"
parent: "Filtering functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/filtering/retain-alpha-numeric/
tags:
  - functions
  - text/filtering
generated: true
---

```
text →
retain-alpha-numeric() → text
```

Returns the input string with all characters removed except for letters (A-Z, a-z) and digits (0-9). If the argument is `null`, it returns `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | retain-alpha-numeric → "HelloWorld"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/filtering`  
**Aliases:** `text-to-retain-alpha-numeric`
{: .member-reference }
