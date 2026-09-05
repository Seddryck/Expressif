---
layout: docs
title: "nor"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 100
has_toc: false
permalink: /predicates/boolean/nor/
tags:
  - predicates
  - boolean
generated: true
---

```
boolean →
nor(
    expression: boolean
) → boolean
```

Returns the negation of the logical disjunction of the Boolean input and a secondary Boolean expression. Evaluates the secondary expression only when the input is `false`.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `boolean` | Yes | Specifies the secondary Boolean expression evaluated when the input is `false`. |






## Examples

{% raw %}
```expressif
#false | nor(#false) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
