---
layout: docs
title: "Classification functions"
parent: "Numeric functions"
grand_parent: "Functions library"
nav_order: 10
has_children: true
has_toc: false
permalink: /functions/numeric/classification/
tags:
  - functions
  - numeric
  - classification
generated: true
---

Reference documentation for Expressif functions in the `numeric/classification` scope.

| Name | Overview |
|:-----|:---------|
| [`bucket`]({{ '/functions/numeric/classification/bucket/' | relative_url }}) | Classifies a numeric value into an equal-width bucket within a half-open interval. Returns `null` when the value is outside the interval or the bucket configuration is invalid. |
| [`bucket-with-outliers`]({{ '/functions/numeric/classification/bucket-with-outliers/' | relative_url }}) | Classifies a numeric value into an equal-width bucket, using additional buckets for values below and above the configured interval. Returns `null` when the bucket configuration is invalid. |
