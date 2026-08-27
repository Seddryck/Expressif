---
layout: docs
title: "Filtering functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 70
has_children: true
has_toc: false
permalink: /functions/text/filtering/
tags:
  - functions
  - text
  - filtering
generated: true
---

Reference documentation for Expressif functions in the `text/filtering` scope.

| Name | Overview |
|:-----|:---------|
| [`filter-chars`]({{ '/functions/text/filtering/filter-chars/' | relative_url }}) | Returns only those characters specified in the parameter, in the order, they were originally entered in the input value. |
| [`retain-alpha`]({{ '/functions/text/filtering/retain-alpha/' | relative_url }}) | Returns the input string with all characters removed except for letters (A-Z, a-z). If the argument is `null`, it returns `null`. |
| [`retain-alpha-numeric`]({{ '/functions/text/filtering/retain-alpha-numeric/' | relative_url }}) | Returns the input string with all characters removed except for letters (A-Z, a-z) and digits (0-9). If the argument is `null`, it returns `null`. |
| [`retain-numeric`]({{ '/functions/text/filtering/retain-numeric/' | relative_url }}) | Returns the input string with all non-numeric characters removed, leaving only digits (0-9).. If the argument is `null`, it returns `null`. |
| [`retain-numeric-symbol`]({{ '/functions/text/filtering/retain-numeric-symbol/' | relative_url }}) | Returns the input string with all characters removed except for digits (0-9) and the symbols `+`, `-`, `,` and `.` If the argument is `null`, it returns `null`. |
