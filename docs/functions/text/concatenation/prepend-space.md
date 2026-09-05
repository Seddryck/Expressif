---
layout: docs
title: "prepend-space"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 90
has_toc: false
permalink: /functions/text/concatenation/prepend-space/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prepend-space() → text
```

Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.

> **Deprecated:** Use `prefix-space` instead. This function is planned for removal in Expressif 3.0.

## Parameters



This function has no parameters.


## Behavior

Deprecated in favor of `prefix-space` and planned for removal in Expressif 3.0. A direct replacement changes null handling because `prefix-space` preserves `null`. Use `null-to-empty | prefix-space` to retain the existing behavior for null input.




## Examples

{% raw %}
```expressif
"Hello World" | prepend-space → " Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prepend-space`
{: .member-reference }
