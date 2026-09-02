---
layout: docs
title: "transform-with"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/flow/transform-with/
tags:
  - functions
  - flow
generated: true
---

```
any →
transform-with(
    operation: expression,
    ...expressions: expression
) → tuple
```

Transforms the results of one or more expressions with the same open expression and returns them as a tuple.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Open expression evaluated once against each result. |
| `expressions` | `expression` | Variadic (one or more) | One or more expressions evaluated independently against the original input. |





## Behavior

`transform-with` evaluates every variadic expression independently against the original input, then evaluates the shared open expression against each resulting value. Results preserve expression order and are always returned as a tuple, including for a single expression. Unlike `apply`, the shared expression is evaluated separately for each result rather than once against the input. Unlike collection mapping, the expressions define the values to transform and the input need not be a collection.



## Examples

{% raw %}
```expressif
{first-name := " Alice ", last-name := " Smith "} | transform-with(trim, .first-name, .last-name) → T("Alice", "Smith")
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
