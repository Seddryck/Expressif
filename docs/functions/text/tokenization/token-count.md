---
layout: docs
title: "token-count"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/tokenization/token-count/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
token-count(
    separator: text
) → integer
```

Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `separator` | `text` | Yes | A character that delimits the substrings in this instance. |





## Examples

{% raw %}
```expressif
"Hello World" | token-count(" ") → 2
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-token-count`
{: .member-reference }
