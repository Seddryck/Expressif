---
layout: docs
title: "Partitioning functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 110
has_children: true
has_toc: false
permalink: /functions/text/partitioning/
tags:
  - functions
  - text
  - partitioning
generated: true
---

Reference documentation for Expressif functions in the `text/partitioning` scope.

| Name | Overview |
|:-----|:---------|
| [`split-lengths`]({{ '/functions/text/partitioning/split-lengths/' | relative_url }}) | Splits text into consecutive nonempty segments of the requested lengths, preserving any remaining text as a final segment. Returns an empty array for null or empty input and null for invalid lengths. |
| [`split-while`]({{ '/functions/text/partitioning/split-while/' | relative_url }}) | Splits text into consecutive nonempty segments while an operation over the current segment and next character returns true. Preserves every character. Returns an empty array for null or empty input and null for a non-Boolean operation result. |
