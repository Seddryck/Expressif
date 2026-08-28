---
layout: docs
title: "text"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 140
has_toc: false
permalink: /functions/text/concatenation/text/
tags:
  - functions
  - text/concatenation
generated: true
---

```
any →
text(
    ...values: expression
) → text
```

Constructs text by evaluating zero or more positional expressions from left to right against the same input, converting each result to text, and concatenating the converted values in order. Spread arguments expand array values in place. Returns empty text when no expressions are supplied.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `values` | `expression` | Variadic (zero or more) | Zero or more expressions whose results are converted to text and concatenated in declaration order. Spread arguments expand array values in place. |





## Examples

{% raw %}
```expressif
#null | text() → ""
#null | text("foo", "bar") → "foobar"
{"Nikola", "Tesla"} | text("foo", ..., "bar") → "fooNikolaTeslabar"
{firstName := "John", lastName := "Doe"} | text(.lastName, ", ", .firstName) → "Doe, John"
```
{% endraw %}


**Kind:** Function
**Scope:** `text/concatenation`
**Aliases:** None
{: .member-reference }
