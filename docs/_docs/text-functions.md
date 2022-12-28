---
title: Text functions
subtitle: Functions applicable to text values
tags: [functions, text]
keywords: [after-substring, before-substring, empty-to-null, first-chars, html-to-text, last-chars, length, lower, mask-to-text, null-to-empty, pad-center, pad-left, pad-right, prefix, remove-chars, skip-first-chars, skip-last-chars, suffix, text-to-datetime, text-to-html, text-to-mask, token, token-count, trim, upper, whitespaces-to-empty, whitespaces-to-null, without-diacritics, without-whitespaces] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### after-substring
###### Overview

Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.

###### Parameters
* substring: The string to seek.
* count (optional) : The number of character positions to examine.

##### before-substring
###### Overview

Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.

###### Parameters
* substring: The string to seek.
* count (optional) : The number of character positions to examine.

##### empty-to-null
###### Overview

Returns the argument value except if this value is `empty` then it returns `null`.

##### first-chars
###### Overview

Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to return

##### html-to-text
###### Overview

Returns the argument value that has previously been HTML-encoded into a decoded string.

##### last-chars
###### Overview

Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to return

##### length
###### Overview

Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`.

##### lower
###### Overview

Returns the argument value converted to lowercase using the casing rules of the invariant culture.

##### mask-to-text
###### Overview

Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`.

###### Parameter
* mask: The string representing the mask to be unset from the argument string

##### null-to-empty
###### Overview

Returns the argument value except if this value is `null` then it returns `empty`.

##### pad-center
###### Overview

Returns a new string that center-aligns the characters in this string by padding them on both the left and the right with a specified character, for a specified total length. If the padding cannot be symetrical then the padding char is added on the right. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

###### Parameters
* length: An integer value between 0 and +Infinity, defining the minimal length of the string returned
* character: The padding character

##### pad-left
###### Overview

Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

###### Parameters
* length: An integer value between 0 and +Infinity, defining the minimal length of the string returned
* character: The padding character

##### pad-right
###### Overview

Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

###### Parameters
* length: An integer value between 0 and +Infinity, defining the minimal length of the string returned
* character: The padding character

##### prefix
###### Overview

Returns the argument value preceeded by the parameter value.

###### Parameter
* prefix: The text to append

##### remove-chars
###### Overview

Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.

###### Parameter
* charToRemove: The char to be removed from the argument string

##### skip-first-chars
###### Overview

Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to skip

##### skip-last-chars
###### Overview

Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

###### Parameter
* length: An integer value between 0 and +Infinity, defining the length of the substring to skip

##### suffix
###### Overview

Returns the argument value followed by the parameter value.

###### Parameter
* suffix: The text to append

##### text-to-datetime
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
* mask: The string representing the mask to apply to the argument string

##### token
###### Overview

Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

###### Parameters
* index: An integer value between 0 and +Infinity, defining the position of the token to be returned.
* separator (optional) : A character that delimits the substrings in this instance.

##### token-count
###### Overview

Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

###### Parameter
* separator: A character that delimits the substrings in this instance.

##### trim
###### Overview

Returns the argument value without all leading or trailing white-space characters.

##### upper
###### Overview

Returns the argument value converted to uppercase using the casing rules of the invariant culture.

##### whitespaces-to-empty
###### Overview

Returns the argument value except if this value only contains white-space characters then it returns `empty`.

##### whitespaces-to-null
###### Overview

Returns the argument value except if this value only contains white-space characters then it returns `null`.

##### without-diacritics
###### Overview

Returns the argument string without diacritics

##### without-whitespaces
###### Overview

Returns the argument string without white-space characters

<!-- END AUTO-GENERATED -->
