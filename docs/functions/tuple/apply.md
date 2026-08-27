---
layout: docs
title: "apply"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/tuple/apply/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
apply(
    expression: expression
) → any
```

Evaluates an expression with the input tuple as its positional context.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Specifies the expression evaluated against the tuple. |






## Examples

{% raw %}
```expressif
T(10, 20) | apply($0 | add($1)) → 30
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `apply`
{: .member-reference }
