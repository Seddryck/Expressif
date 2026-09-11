---
layout: docs
title: "chunk-while"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/partitioning/chunk-while/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
chunk-while(
    operation: expression
) → array
```

Groups consecutive values while an operation over the complete current chunk and next candidate evaluates to `true`. Returns `null` when the operation does not produce a Boolean value or the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the callable or open expression that decides whether the candidate extends the current chunk. |



## Argument evaluation

Visits consecutive elements of the array supplied as pipeline input to this chunk-while call, in source order, starting with the second element.

- **`operation`:** Evaluated once for each candidate after the first element, with T(currentChunk, candidate) as its input and argument context; $0 resolves to a stable array of elements already accepted into this chunk, excluding the candidate, and $1 to the candidate. Empty and singleton arrays do not evaluate the operation.


## Behavior

The first element seeds the first chunk without invoking the operation. For every subsequent candidate, true appends it; false emits the chunk and seeds the next chunk with that candidate without testing it again. The final nonempty chunk is emitted. Every element, including null and structured values, is preserved once and in order; no empty chunk is produced. Later additions do not change an exposed currentChunk array. A non-Boolean result or unsupported input returns null.

The operation receives T(currentChunk, candidate). ~f invokes candidate | f(currentChunk), while f~ invokes currentChunk | f(candidate); subsequent stages retain that tuple as their argument context. Existing bare callable injection uses candidate | f(currentChunk). Pairwise conditions must explicitly select the last element of $0; the previous/current element contract is replaced.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4, 5, 6, 7} | chunk-while($0 | cardinality | less-than(3)) → {{1, 2, 3}, {4, 5, 6}, {7}}
{1, 3, 5, 10, 11} | chunk-while($1 | subtract($0 | last) | absolute | less-than(3)) → {{1, 3, 5}, {10, 11}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
