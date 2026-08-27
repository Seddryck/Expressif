---
layout: docs
title: "retain-numeric-symbol"
parent: "Filtering functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/filtering/retain-numeric-symbol/
tags:
  - functions
  - text/filtering
generated: true
---

```
text →
retain-numeric-symbol() → text
```

Returns the input string with all characters removed except for digits (0-9) and the symbols `+`, `-`, `,` and `.` If the argument is `null`, it returns `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | retain-numeric-symbol → "(empty)"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/filtering`  
**Aliases:** `text-to-retain-numeric-symbol`
{: .member-reference }
