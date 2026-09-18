---
layout: docs
title: "parse-json"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/text/parse-json/
tags:
  - functions
  - text
generated: true
---

```
text →
parse-json() → any
```

Parses JSON text into native records, arrays, and scalar values.



## Parameters



This function has no parameters.





## Behavior

The output type depends on the JSON value at runtime. Objects become ordered records recursively; arrays become native arrays. Null input returns null. Empty, blank, and malformed JSON raise an evaluation error. Comments, trailing commas, and multiple root values are rejected. Numbers follow JSON source conversion rules; duplicate object properties retain the last value.



## Examples

{% raw %}
```expressif
{payload := "{\"name\":\"Ada\"}"} | .payload | parse-json | field("name") → "Ada"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
