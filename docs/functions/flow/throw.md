---
layout: docs
title: "throw"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/flow/throw/
tags:
  - functions
  - flow
generated: true
---

```
any →
throw(
    predicate?: predicate
) → any
```

Raises an evaluation exception when the input is rejected; otherwise, passes the input through.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicate` | `predicate` | No | Predicate that rejects the input when true; defaults to is-null when omitted. |



## Argument evaluation

- **`predicate`:** Evaluated once against the value entering this throw call, replacing the enclosing context. Field and tuple references read that incoming value.


## Behavior

Without a predicate, rejects values matching is-null. A supplied predicate replaces that check completely. The output preserves the input type when accepted; rejection or a predicate error stops evaluation immediately.



## Examples

{% raw %}
```expressif
12.346 | throw(is-negative) | round(2) → 12.35
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
