---
layout: docs
title: "key"
parent: "Grouping functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/grouping/key/
tags:
  - functions
  - array/grouping
generated: true
---

```
any →
key(
    ...expressions: expression
) → pair
```

Associates the input value with a key calculated by one or more expressions.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expressions` | `expression` | Variadic (one or more) | One or more expressions evaluated against the input; multiple results form a tuple key. |






## Examples

{% raw %}
```expressif
"Belgium" | key(@_) → ("Belgium" => "Belgium")
"Alice" | key(upper, lower) → (T("ALICE", "alice") => "Alice")
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/grouping`  
**Aliases:** None
{: .member-reference }
