---
layout: docs
title: "prepend-space"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 110
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


> **Deprecated:** Planned for removal in Expressif 3.0.
>
> Use [`prefix-space`]({{ '/functions/text/concatenation/prefix-space/' | relative_url }}) instead. This replacement is not behavior-equivalent.
>
> **Migration:** The replacement preserves null input; run null-to-empty first to retain the deprecated function's behavior.


## Parameters



This function has no parameters.







## Examples

{% raw %}
```expressif
"Hello World" | prepend-space → " Hello World"
```
{% endraw %}

## Behavior

Deprecated in favor of `prefix-space` and planned for removal in Expressif 3.0. A direct replacement changes null handling because `prefix-space` preserves `null`. Use `null-to-empty | prefix-space` to retain the existing behavior for null input.

**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prepend-space`
{: .member-reference }
