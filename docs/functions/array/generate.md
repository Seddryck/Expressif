---
layout: docs
title: "generate"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/array/generate/
tags:
  - functions
  - array
generated: true
---

```
any →
generate(
    while: predicate,
    next: expression,
    result?: expression
) → array
```

Generates an array by repeatedly transforming a seed while a condition is satisfied.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `while` | `predicate` | Yes | Specifies the predicate that determines whether the current seed is included. |
| `next` | `expression` | Yes | Specifies the expression that produces the next seed. |
| `result` | `expression` | No | Specifies the expression that produces the value appended for the current seed. |





## Behavior

`generate` is type-agnostic. Its input seed, successive values produced by `next`, and optional projected values may use any supported type; evaluation stops when `while` returns `false`.



## Examples

{% raw %}
```expressif
1 | generate(while := less-than-or-equal(3), next := add(1)) → {1, 2, 3}
#"2026-09-01" | generate(while := before(#"2026-09-06"), next := next-day, result := coerce-date) → {#"2026-09-01", #"2026-09-02", #"2026-09-03", #"2026-09-04", #"2026-09-05"}
#"2026-09-01" | generate(while := before(#"2026-09-01T01:00:00"), next := forward(#"00:15:00"), result := coerce-time) → {#"00:00:00", #"00:15:00", #"00:30:00", #"00:45:00"}
#"2026-09-01" | generate(while := before(#"2026-09-06"), next := next-day, result := record(date := coerce-date, day := day-of-month, weekday := day-of-week)) → {{date := #"2026-09-01", day := 1, weekday := 2}, {date := #"2026-09-02", day := 2, weekday := 3}, {date := #"2026-09-03", day := 3, weekday := 4}, {date := #"2026-09-04", day := 4, weekday := 5}, {date := #"2026-09-05", day := 5, weekday := 6}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `generate`
{: .member-reference }
