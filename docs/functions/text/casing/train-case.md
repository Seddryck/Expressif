---
layout: docs
title: "train-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 180
has_toc: false
permalink: /functions/text/casing/train-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
train-case() → text
```

Returns the input text in Train-Case, capitalizing each word and joining them with hyphens. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | train-case → "Hello-World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-train-case`
{: .member-reference }
