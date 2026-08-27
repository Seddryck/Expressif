---
layout: docs
title: "matches-date"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 130
has_toc: false
permalink: /predicates/text/matches-date/
tags:
  - predicates
  - text
generated: true
---

```
matches-date()
```

Returns `true` if the text value passed as argument is a valid representation of a date in the culture specified as parameter. If the value is of type `DateTime` and the time part is set to midnight then it returns `true`. If the value is of type `Date`. Returns `false` otherwise.

## Parameters



This predicate has no parameters.





**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-matches-date`
{: .member-reference }
