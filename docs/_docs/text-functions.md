---
title: Text functions
subtitle: Functions applicable to text values
tags: [functions, text]
---
<!-- START AUTO-GENERATED -->
### empty-to-null

Returns the argument value except if this value is `empty` then it returns `null`.

### first-chars

Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

### html-to-text

Returns the argument value that has previously been HTML-encoded into a decoded string.

### last-chars

Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

### length

Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`.

### lower

Returns the argument value converted to lowercase using the casing rules of the invariant culture.

### mask-to-text

Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`.

### null-to-empty

Returns the argument value except if this value is `null` then it returns `empty`.

### pad-left

Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

### pad-right

Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

### prefix

Returns the argument value preceeded by the parameter value.

### remove-chars

Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.

### skip-first-chars

Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

### skip-last-chars

Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

### suffix

Returns the argument value followed by the parameter value.

### text-to-after

Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.

### text-to-before

Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.

### text-to-datetime

Returns a dateTime value matching the argument value parsed by the long format in the culture specified in parameter.

### text-to-html

Returns the argument value converted to an HTML-encoded string

### text-to-mask

Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced.

### token

Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

### token-count

Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

### trim

Returns the argument value without all leading or trailing white-space characters.

### upper

Returns the argument value converted to uppercase using the casing rules of the invariant culture.

### whitespaces-to-empty

Returns the argument value except if this value only contains white-space characters then it returns `empty`.

### whitespaces-to-null

Returns the argument value except if this value only contains white-space characters then it returns `null`.

### without-diacritics

Returns the argument string without diacritics

### without-whitespaces

Returns the argument string without white-space characters

<!-- END AUTO-GENERATED -->
