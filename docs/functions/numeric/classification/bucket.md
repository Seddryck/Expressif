---
layout: docs
title: "bucket"
parent: "Classification functions"
grand_parent: "Numeric functions"
nav_order: 10
has_toc: false
permalink: /functions/numeric/classification/bucket/
tags:
  - functions
  - numeric/classification
generated: true
---

```
numeric →
bucket(
    minimum: numeric,
    maximum: numeric,
    count: integer
) → integer
```

Classifies a numeric value into an equal-width bucket within a half-open interval. Returns `null` when the value is outside the interval or the bucket configuration is invalid.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `minimum` | `numeric` | Yes | Inclusive lower bound of the classified interval. |
| `maximum` | `numeric` | Yes | Exclusive upper bound of the classified interval. |
| `count` | `integer` | Yes | Strictly positive number of equal-width buckets. |






## Examples

{% raw %}
```expressif
12500 | bucket(5000, 20000, 3) → 2
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/classification`  
**Aliases:** None
{: .member-reference }
