---
layout: docs
title: "summarize"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/grouping/summarize/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
summarize(
    expression: expression
) → dictionary
```

Evaluates an expression once for each group and returns a dictionary from group keys to summary values.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | The expression evaluated against each group's value collection. |






## Examples

{% raw %}
```expressif
#{("BE" => {10, 20, 30}), ("FR" => {5, 15})} | summarize(sum) → !{("BE" => 60), ("FR" => 20)}
#{} | summarize(cardinality) → !{}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
