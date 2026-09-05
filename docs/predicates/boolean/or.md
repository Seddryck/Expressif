---
layout: docs
title: "or"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 90
has_toc: false
permalink: /predicates/boolean/or/
tags:
  - predicates
  - boolean
generated: true
---

```
or(
    expression: any
)
```

Returns the logical disjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `false`.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `any` | Yes | Specifies the secondary predicate expression evaluated when the converted input is `false`. |






## Examples

{% raw %}
```expressif
#true | or(add(1)) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
