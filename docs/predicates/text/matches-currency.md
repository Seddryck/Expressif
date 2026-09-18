---
layout: docs
title: "matches-currency"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 180
has_toc: false
permalink: /predicates/text/matches-currency/
tags:
  - predicates
  - text
generated: true
---

```
text →
matches-currency() → boolean
```

Returns true when the trimmed input is an amount with one Unicode currency symbol at the beginning or end, optional comma grouping, and an optional dot decimal fraction. Returns false for null, empty, blank, and malformed amounts.



## Parameters



This predicate has no parameters.





## Behavior

Requires exactly one symbol from the Unicode CurrencySymbol (Sc) category. Allows whitespace around the whole value and between the symbol and amount, and an optional leading + or - on the numeric amount. Uses ASCII digits, comma grouping with one to three digits in the first group and exactly three in every following group, and a dot fraction with at least one digit on each side. Ungrouped integers may have any length. Rejects internal numeric whitespace, parentheses, currency codes, exponents, repeated symbols, and misplaced signs. This is fixed-format text validation, independent of culture and numeric range; it does not parse or identify a currency.



## Examples

{% raw %}
```expressif
"$1,000.01" | matches-currency → #true
"100.01 €" | matches-currency → #true
"$5,00" | matches-currency → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
