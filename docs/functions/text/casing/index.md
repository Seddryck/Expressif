---
layout: docs
title: "Casing functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 10
has_children: true
has_toc: false
permalink: /functions/text/casing/
tags:
  - functions
  - text
  - casing
generated: true
---

Reference documentation for Expressif functions in the `text/casing` scope.

| Name | Overview |
|:-----|:---------|
| [`allcaps-case`]({{ '/functions/text/casing/allcaps-case/' | relative_url }}) | Returns the input text in ALLCAPS case, uppercasing words and concatenating them without separators. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`camel-case`]({{ '/functions/text/casing/camel-case/' | relative_url }}) | Returns the input text in camelCase, lowercasing the first word and capitalizing subsequent words without separators. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`camel-snake-case`]({{ '/functions/text/casing/camel-snake-case/' | relative_url }}) | Returns the input text in camel_Snake case, lowercasing the first word, capitalizing subsequent words, and joining them with underscores. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`cobol-case`]({{ '/functions/text/casing/cobol-case/' | relative_url }}) | Returns the input text in COBOL-CASE, uppercasing words and joining them with hyphens. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`dot-case`]({{ '/functions/text/casing/dot-case/' | relative_url }}) | Returns the input text in dot.case, lowercasing words and joining them with periods. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`flat-case`]({{ '/functions/text/casing/flat-case/' | relative_url }}) | Returns the input text in flatcase, lowercasing words and concatenating them without separators. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`kebab-case`]({{ '/functions/text/casing/kebab-case/' | relative_url }}) | Returns the input text in kebab-case, lowercasing words and joining them with hyphens. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`lower`]({{ '/functions/text/casing/lower/' | relative_url }}) | Returns the input text converted to lowercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`namespace-case`]({{ '/functions/text/casing/namespace-case/' | relative_url }}) | Returns the input text in namespace::case, lowercasing words and joining them with double colons. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`pascal-case`]({{ '/functions/text/casing/pascal-case/' | relative_url }}) | Returns the input text in PascalCase, capitalizing each word and removing separators. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`pascal-snake-case`]({{ '/functions/text/casing/pascal-snake-case/' | relative_url }}) | Returns the input text in Pascal_Snake case, capitalizing each word and joining them with underscores. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`path-case`]({{ '/functions/text/casing/path-case/' | relative_url }}) | Returns the input text in path/case, lowercasing words and joining them with slashes. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`screaming-snake-case`]({{ '/functions/text/casing/screaming-snake-case/' | relative_url }}) | Returns the input text in SCREAMING_SNAKE_CASE, uppercasing words and joining them with underscores. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`sentence-case`]({{ '/functions/text/casing/sentence-case/' | relative_url }}) | Returns the input text in sentence case by capitalizing the first word while preserving the remaining content. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `AT&T`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`snake-case`]({{ '/functions/text/casing/snake-case/' | relative_url }}) | Returns the input text in snake_case, lowercasing words and joining them with underscores. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`swap-case`]({{ '/functions/text/casing/swap-case/' | relative_url }}) | Returns the input text with lowercase characters converted to uppercase and uppercase characters converted to lowercase. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`title-case`]({{ '/functions/text/casing/title-case/' | relative_url }}) | Returns the input text in title case, capitalizing words while keeping small words lowercase only when they are neither first nor last and do not follow a colon. The first and last words are always capitalized, and a small word after a colon is capitalized. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `Q&A`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`train-case`]({{ '/functions/text/casing/train-case/' | relative_url }}) | Returns the input text in Train-Case, capitalizing each word and joining them with hyphens. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array. |
| [`upper`]({{ '/functions/text/casing/upper/' | relative_url }}) | Returns the input text converted to uppercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
