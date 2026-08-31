---
layout: docs
title: "walk"
parent: "Structure functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/structure/walk/
tags:
  - functions
  - structure
generated: true
---

```
any ΓåÆ
walk(
    transformation: expression
) ΓåÆ any
```

Recursively traverses arrays, tuples, and records and evaluates an expression against each leaf value while preserving container shape.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `transformation` | `expression` | Yes | Expression evaluated against every leaf value. |





## Behavior

`walk` recursively visits array items, tuple items, and record field values. Record field names and container kinds are preserved, and only leaves are supplied to the transformation. The transformation retains its ordinary semantics, so `walk(trim)` permits normal coercion while `walk(*trim)` uses guarded entry.



## Examples

{% raw %}
```expressif
T(42, " 42 ") | walk(trim) ΓåÆ T("42", "42")
T(42, " 42 ") | walk(*trim) ΓåÆ T(42, "42")
{name := " Bob ", address := {city := " Brussels "}} | walk(*trim) ΓåÆ {name := "Bob", address := {city := "Brussels"}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `structure`  
**Aliases:** None
{: .member-reference }
