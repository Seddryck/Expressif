---
layout: docs
title: "Selection functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 110
has_children: true
has_toc: false
permalink: /functions/text/selection/
tags:
  - functions
  - text
  - selection
generated: true
---

Reference documentation for Expressif functions in the `text/selection` scope.

| Name | Overview |
|:-----|:---------|
| [`after-substring`]({{ '/functions/text/selection/after-substring/' | relative_url }}) | Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned. |
| [`before-substring`]({{ '/functions/text/selection/before-substring/' | relative_url }}) | Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`. |
| [`first-chars`]({{ '/functions/text/selection/first-chars/' | relative_url }}) | Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned. |
| [`last-chars`]({{ '/functions/text/selection/last-chars/' | relative_url }}) | Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned. |
| [`skip-first-chars`]({{ '/functions/text/selection/skip-first-chars/' | relative_url }}) | Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. |
| [`skip-last-chars`]({{ '/functions/text/selection/skip-last-chars/' | relative_url }}) | Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. |
