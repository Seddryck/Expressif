---
layout: docs
title: "apply"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/flow/apply/
tags:
  - functions
  - flow
generated: true
---

```
any →
apply(
    expression: expression
) → any
```

Evaluates an expression with the input value as its current context.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Specifies the expression evaluated against the input value. |





## Behavior

`apply` establishes an evaluation boundary that makes its input value current while evaluating its expression. Use it when the child expression contains contextual references or deferred arguments that must resolve against that value. Positional references address tuple elements, field references address record fields, and deferred array expressions consume the current array.



## Examples

{% raw %}
```expressif
T(10, 20) | apply($0 | add($1)) → 30
T(2, 3) | apply(5 | power($0) | add(2) | nth-root($1)) → 3
{firstName := "John", lastName := "Doe"} | apply(.firstName | append-space | append(.lastName)) → "John Doe"
{1, 2, 3, 4} | apply(zip(lag | lag)) → {T(1, #null), T(2, #null), T(3, 1), T(4, 2)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** `apply`
{: .member-reference }
