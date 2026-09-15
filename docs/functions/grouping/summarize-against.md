---
layout: docs
title: "summarize-against"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/grouping/summarize-against/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
summarize-against(
    local: accumulator,
    global: accumulator,
    combine: expression
) → dictionary
```

Summarizes each group against one summary of all grouped values and returns an ordered dictionary.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `local` | `accumulator` | Yes | The accumulator applied independently to each group's values. |
| `global` | `accumulator` | Yes | The accumulator applied once across every group's values. |
| `combine` | `expression` | Yes | The operation combining a finalized local summary with the global summary. |



## Argument evaluation

Visits each value of each group supplied as pipeline input to this call, in group and value order. Each visited value feeds both its group's local accumulator and the shared global accumulator.

- **`local`:** Each value in a group entering this call updates that group's local accumulator once. An empty group still initializes and finalizes its local accumulator.
- **`global`:** Each grouped value also updates the single global accumulator once during the same traversal. The global accumulator is initialized and finalized once, including for an empty grouping.
- **`combine`:** Evaluated once per group after the global accumulator is finalized, with T(local, global) as its pipeline input and argument context. $0 is that group's finalized local value and $1 is the finalized global value; an empty grouping never evaluates combine.


## Behavior

Each grouped value updates its group's local accumulator and the shared global accumulator in one traversal. The global result is finalized once, then each local result is finalized and combined in key order. Accumulator null handling and failures are preserved; null combine results remain dictionary values.



## Examples

{% raw %}
```expressif
#{("BE" => {100, 50}), ("FR" => {50}), ("DE" => {100})} | summarize-against(sum, sum, divide~) → !{("BE" => 0.5), ("FR" => 0.1666666666666666666666666667), ("DE" => 0.3333333333333333333333333333)}
#{("BE" => {100, 50}), ("FR" => {50}), ("DE" => {100})} | summarize-against(sum, max, divide~) → !{("BE" => 1.5), ("FR" => 0.5), ("DE" => 1)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
