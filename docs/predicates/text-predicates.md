---
layout: docs
title: "Text predicates"
parent: "Predicates library"

nav_order: 30
has_children: true
has_toc: false
permalink: /predicates/text-predicates/
tags:
  - predicates
  - text

generated: true
---

Reference documentation for Expressif predicates in the `text` scope.

| Name | Overview |
|:-----|:---------|
| [`contains`]({{ '/predicates/text/contains/' | relative_url }}) | Returns `true` if the value passed as argument contains, anywhere in the string, the text value passed as parameter. Returns `false` otherwise. |
| [`ends-with`]({{ '/predicates/text/ends-with/' | relative_url }}) | Returns `true` if the value passed as argument ends with the text value passed as parameter. Returns `false` otherwise. |
| [`is-any-of`]({{ '/predicates/text/is-any-of/' | relative_url }}) | Returns `true` if the list of text values passed as parameter contains the text value passed as argument. Returns `false` otherwise. |
| [`is-empty`]({{ '/predicates/text/is-empty/' | relative_url }}) | Returns `true` if argument value has a length of `0`. Return `false` otherwise. |
| [`is-empty-or-null`]({{ '/predicates/text/is-empty-or-null/' | relative_url }}) | Returns `true` if argument value has a length of `0` or is `null`. Return `false` otherwise. |
| [`is-equivalent-to`]({{ '/predicates/text/is-equivalent-to/' | relative_url }}) | Compare the text value passed as argument and the text value passed as parameter and returns `true` if they are equal. By default the comparison is agnostic of the culture and case-insensitive. |
| [`is-lower-case`]({{ '/predicates/text/is-lower-case/' | relative_url }}) | Returns `true` if all characters of the text value passed as argument are lower-case. The value `null`, `empty` and `whitespace` also returns `true`. Returns `false` otherwise. |
| [`is-sorted-after`]({{ '/predicates/text/is-sorted-after/' | relative_url }}) | Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value. By default the comparison is agnostic of the culture and case-insensitive. |
| [`is-sorted-after-or-equivalent-to`]({{ '/predicates/text/is-sorted-after-or-equivalent-to/' | relative_url }}) | Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value or if the two values are equal. By default the comparison is agnostic of the culture and case-insensitive. |
| [`is-sorted-before`]({{ '/predicates/text/is-sorted-before/' | relative_url }}) | Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted before the parameter value. By default the comparison is agnostic of the culture and case-insensitive. |
| [`is-sorted-before-or-equivalent-to`]({{ '/predicates/text/is-sorted-before-or-equivalent-to/' | relative_url }}) | Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted before the parameter value or if the two values are equal. By default the comparison is agnostic of the culture and case-insensitive. |
| [`is-upper-case`]({{ '/predicates/text/is-upper-case/' | relative_url }}) | Returns `true` if all characters of the text value passed as argument are upper-case. The value `null`, `empty` and `whitespace` also returns `true`. Returns `false` otherwise. |
| [`matches-date`]({{ '/predicates/text/matches-date/' | relative_url }}) | Returns `true` if the text value passed as argument is a valid representation of a date in the culture specified as parameter. If the value is of type `DateTime` and the time part is set to midnight then it returns `true`. If the value is of type `Date`. Returns `false` otherwise. |
| [`matches-datetime`]({{ '/predicates/text/matches-datetime/' | relative_url }}) | Returns `true` if the text value passed as argument is a valid representation of a dateTime in the culture specified as parameter. The expected format is the concatenation of the ShortDatePattern, a space and the LongTimePattern. If the value is of type `DateTime`, it returns `true`. Returns `false` otherwise. |
| [`matches-numeric`]({{ '/predicates/text/matches-numeric/' | relative_url }}) | Returns `true` if the text value passed as argument is a valid representation of a numeric in the culture specified as parameter. Returns `false` otherwise. |
| [`matches-regex`]({{ '/predicates/text/matches-regex/' | relative_url }}) | Returns `true` if the value passed as argument validate the regex passed as parameter. Returns `false` otherwise. |
| [`matches-time`]({{ '/predicates/text/matches-time/' | relative_url }}) | Returns `true` if the text value passed as argument is a valid representation of a time in the culture specified as parameter. The expected format is the LongTimePattern. If the value is of type `TimeOnly`, it returns `true`. Returns `false` otherwise. |
| [`starts-with`]({{ '/predicates/text/starts-with/' | relative_url }}) | Returns `true` if the value passed as argument starts with the text value passed as parameter. Returns `false` otherwise. |
