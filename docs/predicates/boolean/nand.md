---
layout: docs
title: "nand"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 70
has_toc: false
permalink: /predicates/boolean/nand/
tags:
  - predicates
  - boolean
generated: true
---

```
boolean →
nand(
    expression: boolean
) → boolean
```

Returns the negation of the logical conjunction of the Boolean input and a secondary Boolean expression. Evaluates the secondary expression only when the input is `true`.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `boolean` | Yes | Specifies the secondary Boolean expression evaluated when the input is `true`. |






## Examples

{% raw %}
```expressif
#true | nand(#true) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
