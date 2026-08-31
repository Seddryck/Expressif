---
layout: docs
title: "coerce"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/special/coerce/
tags:
  - functions
  - special
generated: true
---

```
any →
coerce(
    ...specifications: type | mapping
) → any
```

Coerces a scalar value or selected tuple and record values to requested Expressif types.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `specifications` | `type | mapping` | Variadic (one or more) | One or more positional type descriptors or selector-to-type mappings. |





## Behavior

Tuple selector mappings use zero-based positions: `$0` selects the first position, `$1` selects the second, and unavailable positions are ignored.



## Examples

{% raw %}
```expressif
"42" | coerce(:integer) → 42
```

```expressif
T("42", "Bob") | coerce($0 -> :integer) → T(42, "Bob")
```

```expressif
T("Bob", "42") | coerce($1 -> :integer) → T("Bob", 42)
```
{% endraw %}


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `special-to-coerce`
{: .member-reference }
