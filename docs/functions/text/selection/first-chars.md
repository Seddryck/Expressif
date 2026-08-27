---
layout: docs
title: "first-chars"
parent: "Selection functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/selection/first-chars/
tags:
  - functions
  - text/selection
generated: true
---

```
text →
first-chars(
    length: integer
) → text
```

Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the length of the substring to return. |





## Examples

{% raw %}
```expressif
"Hello World" | first-chars(2) → "He"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/selection`  
**Aliases:** `text-to-first-chars`
{: .member-reference }
