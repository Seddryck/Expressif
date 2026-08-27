---
layout: docs
title: "Numeric functions"
parent: "Functions library"

nav_order: 30
has_children: true
has_toc: false
permalink: /functions/numeric-functions/
tags:
  - functions
  - numeric

generated: true
---

Reference documentation for Expressif functions in the `numeric` scope.

| Name | Overview |
|:-----|:---------|
| [`absolute`]({{ '/functions/numeric/arithmetic/absolute/' | relative_url }}) | Returns the absolute value of the argument value. |
| [`add`]({{ '/functions/numeric/arithmetic/add/' | relative_url }}) | Returns the sum of the input value and the parameter value. |
| [`ceiling`]({{ '/functions/numeric/rounding/ceiling/' | relative_url }}) | Returns the smallest integer greater than or equal to the argument number. |
| [`clip`]({{ '/functions/numeric/rounding/clip/' | relative_url }}) | Returns the value of an argument number, unless it is smaller than min, in which case it returns min, or greater than max, in which case it returns max. |
| [`cube-power`]({{ '/functions/numeric/arithmetic/cube-power/' | relative_url }}) | Returns the the numeric argument value raised to the cube power. |
| [`cube-root`]({{ '/functions/numeric/arithmetic/cube-root/' | relative_url }}) | Returns cube root of the numeric argument value. |
| [`decrement`]({{ '/functions/numeric/arithmetic/decrement/' | relative_url }}) | Returns the argument number decremented of one unit. |
| [`divide`]({{ '/functions/numeric/arithmetic/divide/' | relative_url }}) | Returns the argument number divided by the parameter value. If the parameter value is `0`, it returns `null`. |
| [`floor`]({{ '/functions/numeric/rounding/floor/' | relative_url }}) | Returns the largest integer less than or equal to the argument number. |
| [`greatest-common-divisor`]({{ '/functions/numeric/arithmetic/greatest-common-divisor/' | relative_url }}) | Returns the greatest common divisor (GCD) of the argument integer and the parameter integer. Returns `null` if the argument is not an integer. |
| [`human-readable-format-binary-bytes`]({{ '/functions/numeric/formatting/human-readable-format-binary-bytes/' | relative_url }}) | Formats a numeric value as binary bytes using IEC prefixes. |
| [`human-readable-format-decimal`]({{ '/functions/numeric/formatting/human-readable-format-decimal/' | relative_url }}) | Formats a numeric value using decimal SI prefixes. |
| [`human-readable-format-decimal-bytes`]({{ '/functions/numeric/formatting/human-readable-format-decimal-bytes/' | relative_url }}) | Formats a numeric value as decimal bytes using SI prefixes. |
| [`increment`]({{ '/functions/numeric/arithmetic/increment/' | relative_url }}) | Returns the argument number incremented of one unit. |
| [`integer`]({{ '/functions/numeric/rounding/integer/' | relative_url }}) | Returns the value of an argument number rounded to the nearest integer. |
| [`invert`]({{ '/functions/numeric/arithmetic/invert/' | relative_url }}) | Returns the reciprocal of the argument number, meaning the result of the division of 1 by the argument number. If the argument value is `0`, it returns `null`. |
| [`lowest-common-multiple`]({{ '/functions/numeric/arithmetic/lowest-common-multiple/' | relative_url }}) | Returns the lowest common multiple (LCM) of the argument integer and the parameter integer. Returns `null` if the argument is not an integer. |
| [`multiply`]({{ '/functions/numeric/arithmetic/multiply/' | relative_url }}) | Returns the argument number multiplied by the parameter value. |
| [`nth-root`]({{ '/functions/numeric/arithmetic/nth-root/' | relative_url }}) | Returns the root specified by the parameter value of the numeric argument value. |
| [`null-to-zero`]({{ '/functions/numeric/conversion/null-to-zero/' | relative_url }}) | Returns the unmodified argument value except if the argument value is `null`, `empty` or `whitespace` then it returns `0`. |
| [`oppose`]({{ '/functions/numeric/arithmetic/oppose/' | relative_url }}) | Returns the integer being the additive inverse of the argument meaning that their sum is equal to zero. The opposite of 0 is 0. |
| [`percent-change`]({{ '/functions/numeric/arithmetic/percent-change/' | relative_url }}) | Returns the percentage change from the previous numeric value to the current input value. Returns `null` when the input or parameter cannot be evaluated or when the previous value is zero. |
| [`power`]({{ '/functions/numeric/arithmetic/power/' | relative_url }}) | Returns the the numeric argument value raised to the power specified by the parameter value. |
| [`round`]({{ '/functions/numeric/rounding/round/' | relative_url }}) | Returns the value of an argument number to the specified number of fractional digits. |
| [`sign`]({{ '/functions/numeric/arithmetic/sign/' | relative_url }}) | Returns an integer that indicates the sign of the argument value. It returns -1 if the value is strictly negative, 0 if the value is 0 and 1 if the value is strictly positive. |
| [`square-power`]({{ '/functions/numeric/arithmetic/square-power/' | relative_url }}) | Returns the the numeric argument value raised to the square power. |
| [`square-root`]({{ '/functions/numeric/arithmetic/square-root/' | relative_url }}) | Returns square root of the numeric argument value. |
| [`subtract`]({{ '/functions/numeric/arithmetic/subtract/' | relative_url }}) | Returns the difference between the argument number and the parameter value. |
