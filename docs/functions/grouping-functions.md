---
layout: docs
title: "Grouping functions"
parent: "Functions library"

nav_order: 10
has_children: true
has_toc: false
permalink: /functions/grouping-functions/
tags:
  - functions
  - grouping

generated: true
---

Reference documentation for Expressif functions in the `grouping` scope.

| Name | Overview |
|:-----|:---------|
| [`drill-down`]({{ '/functions/grouping/drill-down/' | relative_url }}) | Refines each existing group by appending dimensions derived from its values. |
| [`drill-up`]({{ '/functions/grouping/drill-up/' | relative_url }}) | Derives keys from existing grouping keys and merges matching groups into one grouping level. |
| [`drop-empty-groups`]({{ '/functions/grouping/drop-empty-groups/' | relative_url }}) | Removes groups whose value collection contains no items. |
| [`filter-groups`]({{ '/functions/grouping/filter-groups/' | relative_url }}) | Keeps whole groups whose group-level predicate evaluates to true. |
| [`grouping`]({{ '/functions/grouping/grouping/' | relative_url }}) | Constructs a grouping from zero or more pairs. Spread arguments expand arrays of pairs in place. |
| [`map-groups`]({{ '/functions/grouping/map-groups/' | relative_url }}) | Transforms each group's value collection while preserving its key and position. |
| [`roll-up`]({{ '/functions/grouping/roll-up/' | relative_url }}) | Expands a grouping into its original level and progressively coarser prefix levels. |
| [`summarize`]({{ '/functions/grouping/summarize/' | relative_url }}) | Evaluates an expression once for each group and returns a dictionary from group keys to summary values. |
