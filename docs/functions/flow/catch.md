---
layout: docs
title: "catch"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/flow/catch/
tags:
  - functions
  - flow
generated: true
---

```
any →
catch(
    expression: expression
) → any
```

Returns a recovery result and terminates the current pipeline when the input is null; otherwise, passes the input through.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Recovery expression whose result becomes the final result of the current pipeline. |



## Argument evaluation

- **`expression`:** Evaluated once only when the value entering this catch call is null, in the enclosing expression's context. Field references read that enclosing record, and tuple references select positions in that enclosing tuple.


## Behavior

Uses is-null semantics. The output preserves the input type on the pass-through path and depends on the recovery expression otherwise. Recovery errors propagate, and even a null recovery result terminates only the current pipeline.



## Examples

{% raw %}
```expressif
{nickname := #null, name := "Alice"} | .nickname | catch(.name) | upper → "Alice"
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
