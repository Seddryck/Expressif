---
layout: docs
title: "Concatenation functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 30
has_children: true
has_toc: false
permalink: /functions/text/concatenation/
tags:
  - functions
  - text
  - concatenation
generated: true
---

Reference documentation for Expressif functions in the `text/concatenation` scope.

| Name | Overview |
|:-----|:---------|
| [`append`]({{ '/functions/text/concatenation/append/' | relative_url }}) | **Deprecated.** Returns the argument value followed by the parameter value. If the argument is `null`, it returns the text specified as the parameter. |
| [`append-new-line`]({{ '/functions/text/concatenation/append-new-line/' | relative_url }}) | **Deprecated.** Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`append-space`]({{ '/functions/text/concatenation/append-space/' | relative_url }}) | **Deprecated.** Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`prefix`]({{ '/functions/text/concatenation/prefix/' | relative_url }}) | Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns `null`. |
| [`prefix-new-line`]({{ '/functions/text/concatenation/prefix-new-line/' | relative_url }}) | Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`. |
| [`prefix-new-line-if-missing`]({{ '/functions/text/concatenation/prefix-new-line-if-missing/' | relative_url }}) | Prefixes the argument with a CRLF sequence unless it already starts with CRLF. Preserves `null`. |
| [`prefix-space`]({{ '/functions/text/concatenation/prefix-space/' | relative_url }}) | Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`. |
| [`prefix-space-if-missing`]({{ '/functions/text/concatenation/prefix-space-if-missing/' | relative_url }}) | Prefixes the argument with a space character unless it already starts with one. Preserves `null`. |
| [`prepend`]({{ '/functions/text/concatenation/prepend/' | relative_url }}) | **Deprecated.** Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns the text specified as the parameter. |
| [`prepend-new-line`]({{ '/functions/text/concatenation/prepend-new-line/' | relative_url }}) | **Deprecated.** Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`prepend-space`]({{ '/functions/text/concatenation/prepend-space/' | relative_url }}) | **Deprecated.** Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`replace-slice`]({{ '/functions/text/concatenation/replace-slice/' | relative_url }}) | Returns the argument value with a subset of the string substitued by a another string. |
| [`suffix`]({{ '/functions/text/concatenation/suffix/' | relative_url }}) | Returns the argument value followed by the parameter value. If the argument is `null`, it returns `null`. |
| [`suffix-new-line`]({{ '/functions/text/concatenation/suffix-new-line/' | relative_url }}) | Returns the argument value followed by a space character. If the argument is `null`, it returns `null`. |
| [`suffix-new-line-if-missing`]({{ '/functions/text/concatenation/suffix-new-line-if-missing/' | relative_url }}) | Suffixes the argument with a CRLF sequence unless it already ends with CRLF. Preserves `null`. |
| [`suffix-space`]({{ '/functions/text/concatenation/suffix-space/' | relative_url }}) | Returns the argument value followed by a space character. If the argument is `null`, it returns `null`. |
| [`suffix-space-if-missing`]({{ '/functions/text/concatenation/suffix-space-if-missing/' | relative_url }}) | Suffixes the argument with a space character unless it already ends with one. Preserves `null`. |
| [`text`]({{ '/functions/text/concatenation/text/' | relative_url }}) | Constructs text by evaluating zero or more positional expressions from left to right against the same input, converting each result to text, and concatenating the converted values in order. Spread arguments expand array values in place. Returns empty text when no expressions are supplied. |
