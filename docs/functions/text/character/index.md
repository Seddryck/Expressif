---
layout: docs
title: "Character functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 20
has_children: true
has_toc: false
permalink: /functions/text/character/
tags:
  - functions
  - text
  - character
generated: true
---

Reference documentation for Expressif functions in the `text/character` scope.

| Name | Overview |
|:-----|:---------|
| [`chars`]({{ '/functions/text/character/chars/' | relative_url }}) | Returns the characters in the input text as an array in source order. Returns `null` for `null` and an empty array for empty text. |
| [`remove-chars`]({{ '/functions/text/character/remove-chars/' | relative_url }}) | Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`. |
| [`replace-chars`]({{ '/functions/text/character/replace-chars/' | relative_url }}) | Returns the argument value where a specific char has been replaced by another, both specified as parameters. |
