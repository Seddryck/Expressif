---
layout: docs
title: "is-identical-to"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 40
has_toc: false
permalink: /predicates/boolean/is-identical-to/
tags:
  - predicates
  - boolean
generated: true
---

```
is-identical-to(
    reference: boolean
)
```

Returns `true` if the boolean passed as argument has the same value than the boolean passed as parameter. Returns `false` otherwise.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `boolean` | Yes | A boolean value to compare to the argument. |






## Examples

{% raw %}
```expressif
#true | is-identical-to(#true) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** `identical-to`, `boolean-is-identical-to`
{: .member-reference }
