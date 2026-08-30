---
layout: docs
title: "is-type"
parent: "Special predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/special/is-type/
tags:
  - predicates
  - special
generated: true
---

```
any →
is-type(
    type: type
) → boolean
```

Returns whether the input has the requested Expressif runtime type or belongs to the requested type family, without coercion.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `type` | `type` | Yes | Specifies the Expressif type descriptor to test. |






## Examples

{% raw %}
```expressif
42 | is-type(:numeric) → #true
"42" | is-type(:numeric) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `special`  
**Aliases:** None
{: .member-reference }
