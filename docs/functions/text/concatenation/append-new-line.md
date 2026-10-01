---
layout: docs
title: "append-new-line"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/concatenation/append-new-line/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
append-new-line() → text
```

Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter.


> **Deprecated:** Planned for removal in Expressif 3.0.
>
> Use [`suffix-new-line`]({{ '/functions/text/concatenation/suffix-new-line/' | relative_url }}) instead. This replacement is not behavior-equivalent.
>
> **Migration:** The replacement preserves null input; run null-to-empty first to retain the deprecated function's behavior.


## Parameters



This function has no parameters.







## Behavior

Deprecated in favor of `suffix-new-line` and planned for removal in Expressif 3.0. A direct replacement changes null handling because `suffix-new-line` preserves `null`. Use `null-to-empty | suffix-new-line` to retain the existing behavior for null input.



## Examples

{% raw %}
```expressif
"Hello World" | append-new-line → "Hello World" | append-new-line
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-append-new-line`
{: .member-reference }
