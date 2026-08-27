---
layout: docs
title: "Masking functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 80
has_children: true
has_toc: false
permalink: /functions/text/masking/
tags:
  - functions
  - text
  - masking
generated: true
---

Reference documentation for Expressif functions in the `text/masking` scope.

| Name | Overview |
|:-----|:---------|
| [`mask-to-text`]({{ '/functions/text/masking/mask-to-text/' | relative_url }}) | Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`. |
| [`text-to-mask`]({{ '/functions/text/masking/text-to-mask/' | relative_url }}) | Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced. |
