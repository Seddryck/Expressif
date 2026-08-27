---
layout: docs
title: "count-distinct-chars"
parent: "Counting functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/counting/count-distinct-chars/
tags:
  - functions
  - text/counting
generated: true
---

```
text →
count-distinct-chars() → integer
```

Returns the count of distinct chars in the textual argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | count-distinct-chars → 8
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/counting`  
**Aliases:** `text-to-count-distinct-chars`
{: .member-reference }
