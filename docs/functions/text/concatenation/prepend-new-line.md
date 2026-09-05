---
layout: docs
title: "prepend-new-line"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 80
has_toc: false
permalink: /functions/text/concatenation/prepend-new-line/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prepend-new-line() → text
```

Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.

> **Deprecated:** Use `prefix-new-line` instead. This function is planned for removal in Expressif 3.0.

## Parameters



This function has no parameters.


## Behavior

Deprecated in favor of `prefix-new-line` and planned for removal in Expressif 3.0. A direct replacement changes null handling because `prefix-new-line` preserves `null`. Use `null-to-empty | prefix-new-line` to retain the existing behavior for null input.




## Examples

{% raw %}
```expressif
"Hello World" | prepend-new-line → "Hello World" | prepend-new-line
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prepend-new-line`
{: .member-reference }
