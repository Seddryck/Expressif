---
layout: docs
title: "drop-empty-groups"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/grouping/drop-empty-groups/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
drop-empty-groups() → grouping
```

Removes groups whose value collection contains no items.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#{("BE" => {}), ("FR" => {15})} | drop-empty-groups → #{("FR" => {15})}
#{("BE" => {#null})} | drop-empty-groups → #{("BE" => {#null})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
