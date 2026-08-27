---
title: any-to-any
parent: Special functions
function-category: Special function

pipeline:
  type: any

arguments: []

returns:
  type: any

description: >
  Returns the supplied value unchanged.

examples:
  - input: 42
    output: 42

  - input: "Hello"
    output: "Hello"
---

{% include function-meta.html %}

## Syntax

{% include function-signature.html
    input="value"
    function=page.title
%}

{% include function-contract.html
    pipeline=page.pipeline
    arguments=page.arguments
    returns=page.returns
%}

## Examples

{% include function-examples.html
    examples=page.examples
    function=page.title
%}