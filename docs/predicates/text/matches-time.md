---
layout: docs
title: "matches-time"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 170
has_toc: false
permalink: /predicates/text/matches-time/
tags:
  - predicates
  - text
generated: true
---

```
matches-time()
```

Returns `true` if the text value passed as argument is a valid representation of a time in the culture specified as parameter. The expected format is the LongTimePattern. If the value is of type `TimeOnly`, it returns `true`. Returns `false` otherwise.

## Parameters



This predicate has no parameters.





**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-matches-time`
{: .member-reference }
