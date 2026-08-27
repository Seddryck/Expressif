---
layout: docs
title: "root"
parent: "Io functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/io/root/
tags:
  - functions
  - io
generated: true
---

```
text →
root() → text
```

Returns the root directory information of a file path provided as argument. Returns `empty` if path does not contain root directory information or is `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"docs/_data/function.json" | root → ""
```
{% endraw %}


**Kind:** Function  
**Scope:** `io`  
**Aliases:** `path-to-root`
{: .member-reference }
