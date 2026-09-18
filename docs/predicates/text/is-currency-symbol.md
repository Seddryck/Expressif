---
layout: docs
title: "is-currency-symbol"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 50
has_toc: false
permalink: /predicates/text/is-currency-symbol/
tags:
  - predicates
  - text
generated: true
---

```
text →
is-currency-symbol() → boolean
```

Returns true when the input contains exactly one Unicode currency symbol. Returns false for null, empty, blank, and all other text.



## Parameters



This predicate has no parameters.





## Behavior

Recognizes the Unicode CurrencySymbol (Sc) category, including symbols outside the Currency Symbols block and supplementary Unicode code points. Whitespace is not trimmed; currency codes such as USD are not symbols.



## Examples

{% raw %}
```expressif
"$" | is-currency-symbol → #true
"USD" | is-currency-symbol → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
