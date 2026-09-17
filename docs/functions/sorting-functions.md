---
layout: docs
title: "Sorting functions"
parent: "Functions library"

nav_order: 10
has_children: true
has_toc: false
permalink: /functions/sorting-functions/
tags:
  - functions
  - sorting

generated: true
---

Reference documentation for Expressif functions in the `sorting` scope.

| Name | Overview |
|:-----|:---------|
| [`ascending`]({{ '/functions/sorting/ascending/' | relative_url }}) | Returns the sort term with ascending direction enabled. |
| [`compare`]({{ '/functions/sorting/compare/' | relative_url }}) | Compares two values in a selected supported domain and returns their relative ordering. |
| [`compare-date`]({{ '/functions/sorting/compare-date/' | relative_url }}) | Compares two values after date coercion and returns their relative ordering. |
| [`compare-datetime`]({{ '/functions/sorting/compare-datetime/' | relative_url }}) | Compares two values after datetime coercion and returns their relative ordering. |
| [`compare-numeric`]({{ '/functions/sorting/compare-numeric/' | relative_url }}) | Compares two values after numeric coercion and returns their relative ordering. |
| [`compare-ordinal`]({{ '/functions/sorting/compare-ordinal/' | relative_url }}) | Compares two values as text using deterministic ordinal ordering. |
| [`compare-time`]({{ '/functions/sorting/compare-time/' | relative_url }}) | Compares two values after time coercion and returns their relative ordering. |
| [`dense-rank`]({{ '/functions/sorting/dense-rank/' | relative_url }}) | Groups original sort table row values by their one-based dense rank without gaps. |
| [`dense-rank-by`]({{ '/functions/sorting/dense-rank-by/' | relative_url }}) | Groups array elements by their one-based dense rank using typed criteria. |
| [`descending`]({{ '/functions/sorting/descending/' | relative_url }}) | Returns the sort term with descending direction enabled. |
| [`nulls-first`]({{ '/functions/sorting/nulls-first/' | relative_url }}) | Returns the sort term with nulls ordered first. |
| [`nulls-last`]({{ '/functions/sorting/nulls-last/' | relative_url }}) | Returns the sort term with nulls ordered last. |
| [`rank`]({{ '/functions/sorting/rank/' | relative_url }}) | Groups original sort table row values by their one-based SQL rank. |
| [`rank-by`]({{ '/functions/sorting/rank-by/' | relative_url }}) | Groups array elements by their one-based SQL rank using typed criteria. |
| [`sort`]({{ '/functions/sorting/sort/' | relative_url }}) | Stably sorts a normalized sort table and returns its original row values. |
| [`sort-by`]({{ '/functions/sorting/sort-by/' | relative_url }}) | Stably sorts an array by one or more typed criteria while preserving original elements. |
| [`sort-key`]({{ '/functions/sorting/sort-key/' | relative_url }}) | Creates a non-empty ordered sort key from one or more sort terms. |
| [`sort-table`]({{ '/functions/sorting/sort-table/' | relative_url }}) | Normalizes pairs of sort keys and original values into shared headers and data rows. |
| [`sort-term`]({{ '/functions/sorting/sort-term/' | relative_url }}) | Creates a sort term from a value and a tuple-bound ordering comparer. |
