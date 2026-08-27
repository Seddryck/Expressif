---
layout: docs
title: "retain-alpha"
parent: "Filtering functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/filtering/retain-alpha/
tags:
  - functions
  - text/filtering
generated: true
---

```
text →
retain-alpha() → text
```

Returns the input string with all characters removed except for letters (A-Z, a-z). If the argument is `null`, it returns `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | retain-alpha → "HelloWorld"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/filtering`  
**Aliases:** `text-to-retain-alpha`
{: .member-reference }
