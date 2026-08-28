---
layout: docs
title: "with"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/record/with/
tags:
  - functions
  - record
generated: true
---

```
any →
with(
    ...projections: entry,
    body: expression
) → any
```

Evaluates named projections independently against the input, then evaluates a body expression against their temporary record.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `projections` | `entry` | Variadic (one or more) | One or more named projections evaluated independently against the input value. |
| `body` | `expression` | Yes | The final expression evaluated against the temporary projection record. |






## Examples

{% raw %}
```expressif
{firstName := "John", lastName := "Doe"} | with(last-name := .lastName, first-name := .firstName, .last-name | append(", ") | append(.first-name)) → "Doe, John"
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
