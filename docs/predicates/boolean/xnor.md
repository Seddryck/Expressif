---
layout: docs
title: "xnor"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 120
has_toc: false
permalink: /predicates/boolean/xnor/
tags:
  - predicates
  - boolean
generated: true
---

```
boolean →
xnor(
    expression: boolean
) → boolean
```

Returns the negation of the exclusive disjunction of the Boolean input and a secondary Boolean expression. Always evaluates the secondary expression after the input.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `boolean` | Yes | Specifies the secondary Boolean expression evaluated after the input. |






## Examples

{% raw %}
```expressif
#true | xnor(#true) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
