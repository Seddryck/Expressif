---
layout: docs
title: "directory"
parent: "Io functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/io/directory/
tags:
  - functions
  - io
generated: true
---

```
text →
directory() → text
```

Returns the directory information of a file path provided as argument. The value is always ending by `/` character. Returns `empty` if path does not contain root directory information or is `null`.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"docs/_data/function.json" | directory → "docs\\_data\\"
```
{% endraw %}


**Kind:** Function  
**Scope:** `io`  
**Aliases:** `path-to-directory`
{: .member-reference }
