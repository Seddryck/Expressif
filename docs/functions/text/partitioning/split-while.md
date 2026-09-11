---
layout: docs
title: "split-while"
parent: "Partitioning functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/partitioning/split-while/
tags:
  - functions
  - text/partitioning
generated: true
---

```
text →
split-while(
    operation: expression
) → array
```

Splits text into consecutive nonempty segments while an operation over the current segment and next character returns true. Preserves every character. Returns an empty array for null or empty input and null for a non-Boolean operation result.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | The callable or open expression deciding whether the next character extends the current segment. |



## Argument evaluation

Visits each UTF-16 code unit after the first in the text supplied as pipeline input to this split-while call.

- **`operation`:** Evaluated once per candidate with T(current segment, next character) as pipeline input; pipeline selections $0 and $1 read those positions, while argument expressions retain the enclosing expression context. Null, empty and singleton text do not evaluate the operation.


## Behavior

The operation receives `T(currentSegment, candidate)`: `$0` is the complete current segment, excluding the candidate, and `$1` is the next UTF-16 code unit as a one-character string. The first character seeds the segment without invoking the operation. For each later character, `true` appends it and `false` emits the segment and starts another with that character. Each candidate is tested exactly once; no character is discarded and no empty segment is emitted. Empty, null, and singleton input do not invoke the operation. Blank input represents one space; literal whitespace is preserved. A non-Boolean result returns `null`.

Like `chunk-while`, the first position contains the complete current segment, excluding the candidate. Like `split-lengths`, it preserves character order, so concatenating the segments reproduces the input. Character counting follows `first-chars` and `skip-first-chars`.

### Split at vowel/consonant transitions

Continue while the last character and the candidate have the same vowel classification:

```expressif
"bonjour" | split-while(
    !(
        ($0 | last-chars(1) | matches-regex("(?i)^[aeiouy]$"))
        |XOR
        ($1 | matches-regex("(?i)^[aeiouy]$"))
    )
)
→ {"b", "o", "nj", "ou", "r"}
```

`|XOR` detects a classification change and `!` negates the whole comparison. This example assumes unaccented letters, treats `y` as a vowel, and ignores case.

### Split after two or more spaces and trim each segment

A single space does not break the segment. Split before an ASCII letter following at least two literal spaces, then trim each element:

```expressif
"abc def  ghi jkl   mno" | split-while(
    !(
        ($0 | matches-regex(" {2,}$"))
        |AND
        ($1 | matches-regex("^[A-Za-z]$"))
    )
) | map(trim)
→ {"abc def", "ghi jkl", "mno"}
```

Before trimming, the result is `{"abc def  ", "ghi jkl   ", "mno"}`. Splitting preserves the spaces; the explicit `map(trim)` removes them. `|> trim` is equivalent here.

`starts-with~` invokes segment | starts-with(candidate), while `~starts-with` invokes candidate | starts-with(segment). Both directions require a Boolean result; existing complete expressions are not deprecated.



## Examples

{% raw %}
```expressif
"abcdefgh" | split-while($0 | length | is-less-than(3)) → {"abc", "def", "gh"}
"aaabb" | split-while(starts-with~) → {"aaa", "bb"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/partitioning`  
**Aliases:** None
{: .member-reference }
