---
layout: docs
title: "prepend-new-line"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 100
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


> **Deprecated:** Planned for removal in Expressif 3.0.
>
> Use [`prefix-new-line`]({{ '/functions/text/concatenation/prefix-new-line/' | relative_url }}) instead. This replacement is not behavior-equivalent.
>
> **Migration:** The replacement preserves null input; run null-to-empty first to retain the deprecated function's behavior.


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
