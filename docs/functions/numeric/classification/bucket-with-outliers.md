---
layout: docs
title: "bucket-with-outliers"
parent: "Classification functions"
grand_parent: "Numeric functions"
nav_order: 20
has_toc: false
permalink: /functions/numeric/classification/bucket-with-outliers/
tags:
  - functions
  - numeric/classification
generated: true
---

```
numeric →
bucket-with-outliers(
    minimum: numeric,
    maximum: numeric,
    count: integer
) → integer
```

Classifies a numeric value into an equal-width bucket, using additional buckets for values below and above the configured interval. Returns `null` when the bucket configuration is invalid.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `minimum` | `numeric` | Yes | Inclusive lower bound of the classified interval. |
| `maximum` | `numeric` | Yes | Exclusive upper bound of the classified interval. |
| `count` | `integer` | Yes | Strictly positive number of equal-width in-range buckets. |






## Examples

{% raw %}
```expressif
2500 | bucket-with-outliers(5000, 20000, 3) → 0
22500 | bucket-with-outliers(5000, 20000, 3) → 4
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/classification`  
**Aliases:** None
{: .member-reference }
