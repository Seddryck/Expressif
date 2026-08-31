---
layout: docs
title: "token-count-lexical"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/tokenization/token-count-lexical/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
token-count-lexical() → integer
```

Returns the number of lexical tokens in the argument value, including punctuation and symbols.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello, David!" | token-count-lexical → 4
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-token-count-lexical`
{: .member-reference }
