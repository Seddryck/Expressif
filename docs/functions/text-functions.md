---
layout: docs
title: "Text functions"
parent: "Functions library"

nav_order: 20
has_children: true
has_toc: false
permalink: /functions/text-functions/
tags:
  - functions
  - text

generated: true
---

Reference documentation for Expressif functions in the `text` scope.

| Name | Overview |
|:-----|:---------|
| [`after-substring`]({{ '/functions/text/after-substring/' | relative_url }}) | Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned. |
| [`allcaps-case`]({{ '/functions/text/allcaps-case/' | relative_url }}) | Returns the input text in ALLCAPS case, uppercasing words and concatenating them without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`append`]({{ '/functions/text/append/' | relative_url }}) | Returns the argument value followed by the parameter value. If the argument is `null`, it returns the text specified as the parameter. |
| [`append-new-line`]({{ '/functions/text/append-new-line/' | relative_url }}) | Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`append-space`]({{ '/functions/text/append-space/' | relative_url }}) | Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`before-substring`]({{ '/functions/text/before-substring/' | relative_url }}) | Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`. |
| [`camel-case`]({{ '/functions/text/camel-case/' | relative_url }}) | Returns the input text in camelCase, lowercasing the first word and capitalizing subsequent words without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`camel-snake-case`]({{ '/functions/text/camel-snake-case/' | relative_url }}) | Returns the input text in camel_Snake case, lowercasing the first word, capitalizing subsequent words, and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`clean-whitespace`]({{ '/functions/text/normalization/clean-whitespace/' | relative_url }}) | returns the argument with any whitespace replaced by a space character. `\r\n` is considered as a single character. |
| [`cobol-case`]({{ '/functions/text/cobol-case/' | relative_url }}) | Returns the input text in COBOL-CASE, uppercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`collapse-whitespace`]({{ '/functions/text/normalization/collapse-whitespace/' | relative_url }}) | returns the argument with any two or more consecutive whitespaces replaced by the first whitespace in the sequence and trimming the result. `\r\n` is considered as a single character. |
| [`count-distinct-chars`]({{ '/functions/text/count-distinct-chars/' | relative_url }}) | Returns the count of distinct chars in the textual argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`. |
| [`count-substring`]({{ '/functions/text/count-substring/' | relative_url }}) | Returns the count of non-overlapping occurrences of a substring, defined as a parameter, in the argument value. |
| [`dot-case`]({{ '/functions/text/dot-case/' | relative_url }}) | Returns the input text in dot.case, lowercasing words and joining them with periods. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`empty-to-null`]({{ '/functions/text/empty-to-null/' | relative_url }}) | Returns the argument value except if this value is `empty` then it returns `null`. |
| [`filter-chars`]({{ '/functions/text/filter-chars/' | relative_url }}) | Returns only those characters specified in the parameter, in the order, they were originally entered in the input value. |
| [`first-chars`]({{ '/functions/text/first-chars/' | relative_url }}) | Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned. |
| [`flat-case`]({{ '/functions/text/flat-case/' | relative_url }}) | Returns the input text in flatcase, lowercasing words and concatenating them without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`html-to-text`]({{ '/functions/text/html-to-text/' | relative_url }}) | Returns the argument value that has previously been HTML-encoded into a decoded string. |
| [`kebab-case`]({{ '/functions/text/kebab-case/' | relative_url }}) | Returns the input text in kebab-case, lowercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`last-chars`]({{ '/functions/text/last-chars/' | relative_url }}) | Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned. |
| [`length`]({{ '/functions/text/length/' | relative_url }}) | Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`. |
| [`lower`]({{ '/functions/text/lower/' | relative_url }}) | Returns the input text converted to lowercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`mask-to-text`]({{ '/functions/text/mask-to-text/' | relative_url }}) | Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`. |
| [`namespace-case`]({{ '/functions/text/namespace-case/' | relative_url }}) | Returns the input text in namespace::case, lowercasing words and joining them with double colons. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`null-to-empty`]({{ '/functions/text/null-to-empty/' | relative_url }}) | Returns the argument value except if this value is `null` then it returns `empty`. |
| [`pad-center`]({{ '/functions/text/pad-center/' | relative_url }}) | Returns a new string that center-aligns the characters in this string by padding them on both the left and the right with a specified character, for a specified total length. If the padding cannot be symetrical then the padding char is added on the right. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. |
| [`pad-left`]({{ '/functions/text/pad-left/' | relative_url }}) | Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. |
| [`pad-right`]({{ '/functions/text/pad-right/' | relative_url }}) | Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified. |
| [`pascal-case`]({{ '/functions/text/pascal-case/' | relative_url }}) | Returns the input text in PascalCase, capitalizing each word and removing separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`pascal-snake-case`]({{ '/functions/text/pascal-snake-case/' | relative_url }}) | Returns the input text in Pascal_Snake case, capitalizing each word and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`path-case`]({{ '/functions/text/path-case/' | relative_url }}) | Returns the input text in path/case, lowercasing words and joining them with slashes. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`prefix`]({{ '/functions/text/prefix/' | relative_url }}) | Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns `null`. |
| [`prefix-new-line`]({{ '/functions/text/prefix-new-line/' | relative_url }}) | Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`. |
| [`prefix-space`]({{ '/functions/text/prefix-space/' | relative_url }}) | Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`. |
| [`prepend`]({{ '/functions/text/prepend/' | relative_url }}) | Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns the text specified as the parameter. |
| [`prepend-new-line`]({{ '/functions/text/prepend-new-line/' | relative_url }}) | Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`prepend-space`]({{ '/functions/text/prepend-space/' | relative_url }}) | Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter. |
| [`remove-chars`]({{ '/functions/text/remove-chars/' | relative_url }}) | Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`. |
| [`replace-chars`]({{ '/functions/text/replace-chars/' | relative_url }}) | Returns the argument value where a specific char has been replaced by another, both specified as parameters. |
| [`replace-slice`]({{ '/functions/text/replace-slice/' | relative_url }}) | Returns the argument value with a subset of the string substitued by a another string. |
| [`retain-alpha`]({{ '/functions/text/retain-alpha/' | relative_url }}) | Returns the input string with all characters removed except for letters (A-Z, a-z). If the argument is `null`, it returns `null`. |
| [`retain-alpha-numeric`]({{ '/functions/text/retain-alpha-numeric/' | relative_url }}) | Returns the input string with all characters removed except for letters (A-Z, a-z) and digits (0-9). If the argument is `null`, it returns `null`. |
| [`retain-numeric`]({{ '/functions/text/retain-numeric/' | relative_url }}) | Returns the input string with all non-numeric characters removed, leaving only digits (0-9).. If the argument is `null`, it returns `null`. |
| [`retain-numeric-symbol`]({{ '/functions/text/retain-numeric-symbol/' | relative_url }}) | Returns the input string with all characters removed except for digits (0-9) and the symbols `+`, `-`, `,` and `.` If the argument is `null`, it returns `null`. |
| [`screaming-snake-case`]({{ '/functions/text/screaming-snake-case/' | relative_url }}) | Returns the input text in SCREAMING_SNAKE_CASE, uppercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`sentence-case`]({{ '/functions/text/sentence-case/' | relative_url }}) | Returns the input text in sentence case by capitalizing the first word while preserving the remaining content. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `AT&T`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`skip-first-chars`]({{ '/functions/text/skip-first-chars/' | relative_url }}) | Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. |
| [`skip-last-chars`]({{ '/functions/text/skip-last-chars/' | relative_url }}) | Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`. |
| [`snake-case`]({{ '/functions/text/snake-case/' | relative_url }}) | Returns the input text in snake_case, lowercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`suffix`]({{ '/functions/text/suffix/' | relative_url }}) | Returns the argument value followed by the parameter value. If the argument is `null`, it returns `null`. |
| [`suffix-new-line`]({{ '/functions/text/suffix-new-line/' | relative_url }}) | Returns the argument value followed by a space character. If the argument is `null`, it returns `null`. |
| [`suffix-space`]({{ '/functions/text/suffix-space/' | relative_url }}) | Returns the argument value followed by a space character. If the argument is `null`, it returns `null`. |
| [`swap-case`]({{ '/functions/text/swap-case/' | relative_url }}) | Returns the input text with lowercase characters converted to uppercase and uppercase characters converted to lowercase. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`text-to-datetime`]({{ '/functions/text/conversion/text-to-datetime/' | relative_url }}) | Returns a dateTime value matching the argument value parsed by the long format in the culture specified in parameter. |
| [`text-to-html`]({{ '/functions/text/text-to-html/' | relative_url }}) | Returns the argument value converted to an HTML-encoded string |
| [`text-to-mask`]({{ '/functions/text/text-to-mask/' | relative_url }}) | Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced. |
| [`title-case`]({{ '/functions/text/title-case/' | relative_url }}) | Returns the input text in title case, capitalizing words while keeping small words lowercase only when they are neither first nor last and do not follow a colon. The first and last words are always capitalized, and a small word after a colon is capitalized. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `Q&A`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`token`]({{ '/functions/text/token/' | relative_url }}) | Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens. |
| [`token-count`]({{ '/functions/text/token-count/' | relative_url }}) | Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens. |
| [`train-case`]({{ '/functions/text/train-case/' | relative_url }}) | Returns the input text in Train-Case, capitalizing each word and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array. |
| [`trim`]({{ '/functions/text/normalization/trim/' | relative_url }}) | Returns the argument value without all leading or trailing white-space characters. |
| [`upper`]({{ '/functions/text/upper/' | relative_url }}) | Returns the input text converted to uppercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array. |
| [`whitespaces-to-empty`]({{ '/functions/text/normalization/whitespaces-to-empty/' | relative_url }}) | Returns the argument value except if this value only contains white-space characters then it returns `empty`. |
| [`whitespaces-to-null`]({{ '/functions/text/normalization/whitespaces-to-null/' | relative_url }}) | Returns the argument value except if this value only contains white-space characters then it returns `null`. |
| [`without-diacritics`]({{ '/functions/text/normalization/without-diacritics/' | relative_url }}) | Returns the argument string without diacritics. |
| [`without-whitespaces`]({{ '/functions/text/normalization/without-whitespaces/' | relative_url }}) | Returns the argument string without white-space characters. |
