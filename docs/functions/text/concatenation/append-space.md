---
layout: docs
title: "append-space"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/concatenation/append-space/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
append-space() → text
```

Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter.


> **Deprecated:** Planned for removal in Expressif 3.0.
>
> Use [`suffix-space`]({{ '/functions/text/concatenation/suffix-space/' | relative_url }}) instead. This replacement is not behavior-equivalent.
>
> **Migration:** The replacement preserves null input; run null-to-empty first to retain the deprecated function's behavior.


## Parameters



This function has no parameters.







## Behavior

Deprecated in favor of `suffix-space` and planned for removal in Expressif 3.0. A direct replacement changes null handling because `suffix-space` preserves `null`. Use `null-to-empty | suffix-space` to retain the existing behavior for null input.



## Examples

{% raw %}
```expressif
"Hello World" | append-space → "Hello World "
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-append-space`
{: .member-reference }
