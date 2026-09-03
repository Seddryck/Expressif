---
layout: docs
title: "dictionary"
parent: "Dictionary functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/dictionary/dictionary/
tags:
  - functions
  - dictionary
generated: true
---

```
any →
dictionary(
    ...values: pair
) → dictionary
```

Constructs a dictionary from zero or more pairs. Spread arguments expand arrays of pairs in place.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `values` | `pair` | Variadic (zero or more) | Zero or more pairs whose unique keys and values become dictionary entries. |






## Examples

{% raw %}
```expressif
#null | dictionary() → !{}
#null | dictionary(("BE" => "Belgium"), ("FR" => "France")) → !{("BE" => "Belgium"), ("FR" => "France")}
```
{% endraw %}


**Kind:** Function  
**Scope:** `dictionary`  
**Aliases:** None
{: .member-reference }
