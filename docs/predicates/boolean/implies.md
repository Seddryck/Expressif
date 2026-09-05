---
layout: docs
title: "implies"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/boolean/implies/
tags:
  - predicates
  - boolean
generated: true
---

```
boolean →
implies(
    expression: boolean
) → boolean
```

Returns logical implication from the Boolean input to a secondary Boolean expression. Returns `true` without evaluating the expression when the input is `false`.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `boolean` | Yes | Specifies the secondary Boolean expression evaluated when the input is `true`. |






## Examples

{% raw %}
```expressif
#true | implies(#false) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
