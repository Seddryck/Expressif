---
layout: docs
title: "Tokenization functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 120
has_children: true
has_toc: false
permalink: /functions/text/tokenization/
tags:
  - functions
  - text
  - tokenization
generated: true
---

Reference documentation for Expressif functions in the `text/tokenization` scope.

| Name | Overview |
|:-----|:---------|
| [`token`]({{ '/functions/text/tokenization/token/' | relative_url }}) | Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens. |
| [`token-count`]({{ '/functions/text/tokenization/token-count/' | relative_url }}) | Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens. |
| [`token-count-lexical`]({{ '/functions/text/tokenization/token-count-lexical/' | relative_url }}) | Returns the number of lexical tokens in the argument value, including punctuation and symbols. |
| [`tokenize`]({{ '/functions/text/tokenization/tokenize/' | relative_url }}) | Returns all tokens in the argument value in source order. By default, tokenization uses white-space characters as delimiters. If a character is specified, that character delimits the tokens. |
| [`tokenize-lexical`]({{ '/functions/text/tokenization/tokenize-lexical/' | relative_url }}) | Returns lexical tokens in source order, preserving punctuation and symbols as separate tokens. |
