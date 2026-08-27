---
layout: docs
title: "filename-without-extension"
parent: "Io functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/io/filename-without-extension/
tags:
  - functions
  - io
generated: true
---

```
text →
filename-without-extension() → text
```

Returns the file name without the extension of a file path provided as argument.

## Parameters



This function has no parameters.





## Examples

```expressif
"docs/_data/function.json" | filename-without-extension → "function"
```


**Kind:** Function  
**Scope:** `io`  
**Aliases:** `path-to-filename-without-extension`
{: .member-reference }
