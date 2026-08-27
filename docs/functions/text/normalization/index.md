---
layout: docs
title: "Normalization functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 90
has_children: true
has_toc: false
permalink: /functions/text/normalization/
tags:
  - functions
  - text
  - normalization
generated: true
---

Reference documentation for Expressif functions in the `text/normalization` scope.

| Name | Overview |
|:-----|:---------|
| [`clean-whitespace`]({{ '/functions/text/normalization/clean-whitespace/' | relative_url }}) | returns the argument with any whitespace replaced by a space character. `\r\n` is considered as a single character. |
| [`collapse-whitespace`]({{ '/functions/text/normalization/collapse-whitespace/' | relative_url }}) | returns the argument with any two or more consecutive whitespaces replaced by the first whitespace in the sequence and trimming the result. `\r\n` is considered as a single character. |
| [`empty-to-null`]({{ '/functions/text/normalization/empty-to-null/' | relative_url }}) | Returns the argument value except if this value is `empty` then it returns `null`. |
| [`null-to-empty`]({{ '/functions/text/normalization/null-to-empty/' | relative_url }}) | Returns the argument value except if this value is `null` then it returns `empty`. |
| [`slug`]({{ '/functions/text/normalization/slug/' | relative_url }}) | Returns a lowercase, separator-normalized slug, removing Latin diacritics without transliterating non-Latin scripts. Returns empty text when the input is `null`, empty, or blank. |
| [`trim`]({{ '/functions/text/normalization/trim/' | relative_url }}) | Returns the argument value without all leading or trailing white-space characters. |
| [`whitespaces-to-empty`]({{ '/functions/text/normalization/whitespaces-to-empty/' | relative_url }}) | Returns the argument value except if this value only contains white-space characters then it returns `empty`. |
| [`whitespaces-to-null`]({{ '/functions/text/normalization/whitespaces-to-null/' | relative_url }}) | Returns the argument value except if this value only contains white-space characters then it returns `null`. |
| [`without-diacritics`]({{ '/functions/text/normalization/without-diacritics/' | relative_url }}) | Returns the argument string without diacritics. |
| [`without-whitespaces`]({{ '/functions/text/normalization/without-whitespaces/' | relative_url }}) | Returns the argument string without white-space characters. |
