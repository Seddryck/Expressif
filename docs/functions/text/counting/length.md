---
layout: docs
title: "length"
parent: "Counting functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/counting/length/
tags:
  - functions
  - text/counting
generated: true
---

```
text →
length() → integer
```

Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | length → 11
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/counting`  
**Aliases:** `text-to-length`, `count-chars`
{: .member-reference }
