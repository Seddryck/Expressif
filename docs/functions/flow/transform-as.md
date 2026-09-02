---
layout: docs
title: "transform-as"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/flow/transform-as/
tags:
  - functions
  - flow
generated: true
---

```
any →
transform-as(
    operation: expression,
    ...expressions: entry
) → record
```

Transforms one or more named expression results with the same open expression and returns them as a record.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Open expression evaluated once against each named result. |
| `expressions` | `entry` | Variadic (one or more) | One or more named expressions evaluated independently against the original input. |





## Behavior

`transform-as` evaluates every named expression independently against the original input, then evaluates the shared open expression against each resulting value. Each argument name becomes the corresponding field name, declaration order is preserved, and the result is always a record, including for a single named expression. Unnamed expressions after the operation are rejected. `transform-as` is the named, record-producing counterpart of `transform-with`, which accepts positional expressions and returns a tuple. Unlike `apply`, the shared expression is evaluated separately for each named result.



## Examples

{% raw %}
```expressif
{first-name := " Alice ", last-name := " Smith "} | transform-as(trim, first-name := .first-name, last-name := .last-name) → {first-name := "Alice", last-name := "Smith"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
