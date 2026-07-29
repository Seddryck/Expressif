---
title: Text functions
subtitle: Functions applicable to text values
tags: [functions, text]
keywords: [after-substring, allcaps-case, append, append-new-line, append-space, before-substring, camel-case, camel-snake-case, clean-whitespace, cobol-case, collapse-whitespace, count-distinct-chars, count-substring, dot-case, empty-to-null, filter-chars, first-chars, flat-case, html-to-text, kebab-case, last-chars, length, lower, mask-to-text, namespace-case, null-to-empty, pad-center, pad-left, pad-right, pascal-case, pascal-snake-case, path-case, prefix, prefix-new-line, prefix-space, prepend, prepend-new-line, prepend-space, remove-chars, replace-chars, replace-slice, retain-alpha, retain-alpha-numeric, retain-numeric, retain-numeric-symbol, screaming-snake-case, sentence-case, skip-first-chars, skip-last-chars, snake-case, suffix, suffix-new-line, suffix-space, swap-case, text-to-datetime, text-to-html, text-to-mask, title-case, token, token-count, train-case, trim, upper, whitespaces-to-empty, whitespaces-to-null, without-diacritics, without-whitespaces] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### after-substring

###### Alias: `text-to-after-substring`

###### Overview

Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.

###### Parameters
* substring: The string to seek.
* count (optional) : The number of character positions to examine.

##### allcaps-case

###### Alias: `text-to-allcaps-case`

###### Overview

Returns the input text in ALLCAPS case, uppercasing words and concatenating them without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### append

###### Alias: `text-to-append`

###### Overview

Returns the argument value followed by the parameter value. If the argument is `null`, it returns the text specified as the parameter.

###### Parameter
* text: The text to append

##### append-new-line

###### Alias: `text-to-append-new-line`

###### Overview

Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter.

##### append-space

###### Alias: `text-to-append-space`

###### Overview

Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter.

##### before-substring

###### Alias: `text-to-before-substring`

###### Overview

Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.

###### Parameters
* substring: The string to seek.
* count (optional) : The number of character positions to examine.

##### camel-case

###### Alias: `text-to-camel-case`

###### Overview

Returns the input text in camelCase, lowercasing the first word and capitalizing subsequent words without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### camel-snake-case

###### Alias: `text-to-camel-snake-case`

###### Overview

Returns the input text in camel_Snake case, lowercasing the first word, capitalizing subsequent words, and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### clean-whitespace

###### Alias: `text-to-clean-whitespace`

###### Overview

returns the argument with any whitespace replaced by a space character. `\r\n` is considered as a single character.

##### cobol-case

###### Alias: `text-to-cobol-case`

###### Overview

Returns the input text in COBOL-CASE, uppercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### collapse-whitespace

###### Alias: `text-to-collapse-whitespace`

###### Overview

returns the argument with any two or more consecutive whitespaces replaced by the first whitespace in the sequence and trimming the result. `\r\n` is considered as a single character.

##### count-distinct-chars

###### Alias: `text-to-count-distinct-chars`

###### Overview

Returns the count of distinct chars in the textual argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`.

##### count-substring

###### Alias: `text-to-count-substring`

###### Overview

Returns the count of non-overlapping occurrences of a substring, defined as a parameter, in the argument value.

###### Parameter
* substring: The substring to count in the argument value.

##### dot-case

###### Alias: `text-to-dot-case`

###### Overview

Returns the input text in dot.case, lowercasing words and joining them with periods. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### empty-to-null
###### Overview

Returns the argument value except if this value is `empty` then it returns `null`.

##### filter-chars

###### Alias: `text-to-filter-chars`

###### Overview

Returns only those characters specified in the parameter, in the order, they were originally entered in the input value.

###### Parameter
* filter: The chars to filter from the argument string.

##### first-chars

###### Alias: `text-to-first-chars`

###### Overview

Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to return.

##### flat-case

###### Alias: `text-to-flat-case`

###### Overview

Returns the input text in flatcase, lowercasing words and concatenating them without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### html-to-text
###### Overview

Returns the argument value that has previously been HTML-encoded into a decoded string.

##### kebab-case

###### Alias: `text-to-kebab-case`

###### Overview

Returns the input text in kebab-case, lowercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### last-chars

###### Alias: `text-to-last-chars`

###### Overview

Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to return.

##### length

###### Aliases: `text-to-length`, `count-chars`

###### Overview

Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`.

##### lower

###### Alias: `text-to-lower`

###### Overview

Returns the input text converted to lowercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

##### mask-to-text
###### Overview

Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`.

###### Parameter
* mask: The string representing the mask to be unset from the argument string.

##### namespace-case

###### Alias: `text-to-namespace-case`

###### Overview

Returns the input text in namespace::case, lowercasing words and joining them with double colons. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### null-to-empty
###### Overview

Returns the argument value except if this value is `null` then it returns `empty`.

##### pad-center

###### Alias: `text-to-pad-center`

###### Overview

Returns a new string that center-aligns the characters in this string by padding them on both the left and the right with a specified character, for a specified total length. If the padding cannot be symetrical then the padding char is added on the right. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

###### Parameters
* length: An integer value between 0 and +Infinity, defining the minimal length of the string returned
* character: The padding character

##### pad-left

###### Alias: `text-to-pad-left`

###### Overview

Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

###### Parameters
* length: An integer value between 0 and +Infinity, defining the minimal length of the string returned
* character: The padding character

##### pad-right

###### Alias: `text-to-pad-right`

###### Overview

Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

###### Parameters
* length: An integer value between 0 and +Infinity, defining the minimal length of the string returned
* character: The padding character

##### pascal-case

###### Alias: `text-to-pascal-case`

###### Overview

Returns the input text in PascalCase, capitalizing each word and removing separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### pascal-snake-case

###### Alias: `text-to-pascal-snake-case`

###### Overview

Returns the input text in Pascal_Snake case, capitalizing each word and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### path-case

###### Alias: `text-to-path-case`

###### Overview

Returns the input text in path/case, lowercasing words and joining them with slashes. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### prefix

###### Alias: `text-to-prefix`

###### Overview

Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns `null`.

###### Parameter
* prefix: The text to append

##### prefix-new-line

###### Alias: `text-to-prefix-new-line`

###### Overview

Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`.

##### prefix-space

###### Alias: `text-to-prefix-space`

###### Overview

Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`.

##### prepend

###### Alias: `text-to-prepend`

###### Overview

Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns the text specified as the parameter.

###### Parameter
* text: The text to prepend

##### prepend-new-line

###### Alias: `text-to-prepend-new-line`

###### Overview

Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.

##### prepend-space

###### Alias: `text-to-prepend-space`

###### Overview

Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.

##### remove-chars

###### Alias: `text-to-remove-chars`

###### Overview

Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.

###### Parameter
* charToRemove: The char to be removed from the argument string.

##### replace-chars

###### Alias: `text-to-replace-chars`

###### Overview

Returns the argument value where a specific char has been replaced by another, both specified as parameters.

###### Parameters
* charToReplace: The char to be replaced from the argument string.
* charReplacing: The replacing char from the argument string.

##### replace-slice

###### Alias: `text-to-replace-slice`

###### Overview

Returns the argument value with a subset of the string substitued by a another string.

###### Parameters
* start: The position to start to replace
* length: The length to replace
* append: The text to append when the slice has been removed

##### retain-alpha

###### Alias: `text-to-retain-alpha`

###### Overview

Returns the input string with all characters removed except for letters (A-Z, a-z). If the argument is `null`, it returns `null`.

##### retain-alpha-numeric

###### Alias: `text-to-retain-alpha-numeric`

###### Overview

Returns the input string with all characters removed except for letters (A-Z, a-z) and digits (0-9). If the argument is `null`, it returns `null`.

##### retain-numeric

###### Alias: `text-to-retain-numeric`

###### Overview

Returns the input string with all non-numeric characters removed, leaving only digits (0-9).. If the argument is `null`, it returns `null`.

##### retain-numeric-symbol

###### Alias: `text-to-retain-numeric-symbol`

###### Overview

Returns the input string with all characters removed except for digits (0-9) and the symbols `+`, `-`, `,` and `.` If the argument is `null`, it returns `null`.

##### screaming-snake-case

###### Alias: `text-to-screaming-snake-case`

###### Overview

Returns the input text in SCREAMING_SNAKE_CASE, uppercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### sentence-case

###### Aliases: `text-to-sentence-case`, `capitalize`

###### Overview



##### skip-first-chars

###### Alias: `text-to-skip-first-chars`

###### Overview

Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to skip.

##### skip-last-chars

###### Alias: `text-to-skip-last-chars`

###### Overview

Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to skip.

##### snake-case

###### Alias: `text-to-snake-case`

###### Overview

Returns the input text in snake_case, lowercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### suffix

###### Alias: `text-to-suffix`

###### Overview

Returns the argument value followed by the parameter value. If the argument is `null`, it returns `null`.

###### Parameter
* suffix: The text to append

##### suffix-new-line

###### Alias: `text-to-suffix-new-line`

###### Overview

Returns the argument value followed by a space character. If the argument is `null`, it returns `null`.

##### suffix-space

###### Alias: `text-to-suffix-space`

###### Overview

Returns the argument value followed by a space character. If the argument is `null`, it returns `null`.

##### swap-case

###### Alias: `text-to-swap-case`

###### Overview

Returns the input text with lowercase characters converted to uppercase and uppercase characters converted to lowercase. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

##### text-to-datetime

###### Alias: `text-to-dateTime`

###### Overview

Returns a dateTime value matching the argument value parsed by the long format in the culture specified in parameter.

###### Parameters
* format: A string representing the required format.
* culture (optional) : A string representing a pre-defined culture.

##### text-to-html
###### Overview

Returns the argument value converted to an HTML-encoded string

##### text-to-mask
###### Overview

Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced.

###### Parameter
* mask: The string representing the mask to apply to the argument string.

##### title-case

###### Alias: `text-to-title-case`

###### Overview



##### token

###### Alias: `text-to-token`

###### Overview

Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

###### Parameters
* index: An integer value between 0 and +Infinity, defining the position of the token to be returned.
* separator (optional) : A character that delimits the substrings in this instance.

##### token-count

###### Alias: `text-to-token-count`

###### Overview

Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

###### Parameter
* separator: A character that delimits the substrings in this instance.

##### train-case

###### Alias: `text-to-train-case`

###### Overview

Returns the input text in Train-Case, capitalizing each word and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

##### trim

###### Alias: `text-to-trim`

###### Overview

Returns the argument value without all leading or trailing white-space characters.

##### upper

###### Alias: `text-to-upper`

###### Overview

Returns the input text converted to uppercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

##### whitespaces-to-empty

###### Alias: `blank-to-empty`

###### Overview

Returns the argument value except if this value only contains white-space characters then it returns `empty`.

##### whitespaces-to-null

###### Alias: `blank-to-null`

###### Overview

Returns the argument value except if this value only contains white-space characters then it returns `null`.

##### without-diacritics

###### Alias: `text-to-without-diacritics`

###### Overview

Returns the argument string without diacritics.

##### without-whitespaces

###### Alias: `text-to-without-whitespaces`

###### Overview

Returns the argument string without white-space characters.

<!-- END AUTO-GENERATED -->
