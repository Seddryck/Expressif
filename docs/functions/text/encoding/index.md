---
layout: docs
title: "Encoding functions"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 60
has_children: true
has_toc: false
permalink: /functions/text/encoding/
tags:
  - functions
  - text
  - encoding
generated: true
---

Reference documentation for Expressif functions in the `text/encoding` scope.

| Name | Overview |
|:-----|:---------|
| [`html-to-text`]({{ '/functions/text/encoding/html-to-text/' | relative_url }}) | Returns the argument value that has previously been HTML-encoded into a decoded string. |
| [`json-escaped-to-text`]({{ '/functions/text/encoding/json-escaped-to-text/' | relative_url }}) | Returns text by decoding escaped JSON string contents without requiring surrounding quotation marks. Returns `null` for malformed input and preserves `null`, empty, and blank inputs. |
| [`text-to-html`]({{ '/functions/text/encoding/text-to-html/' | relative_url }}) | Returns the argument value converted to an HTML-encoded string |
| [`text-to-json-escaped`]({{ '/functions/text/encoding/text-to-json-escaped/' | relative_url }}) | Returns the escaped contents of a JSON string without surrounding quotation marks. Preserves `null`, empty, and blank inputs. |
| [`text-to-uri`]({{ '/functions/text/encoding/text-to-uri/' | relative_url }}) | Returns the input text escaped as URI data using UTF-8 percent encoding. Preserves `null`, empty, and blank inputs. |
| [`text-to-xml-escaped`]({{ '/functions/text/encoding/text-to-xml-escaped/' | relative_url }}) | Returns text escaped for use as XML character data without adding a containing element. Returns `null` for characters that are invalid in XML and preserves `null`, empty, and blank inputs. |
| [`uri-to-text`]({{ '/functions/text/encoding/uri-to-text/' | relative_url }}) | Returns text by unescaping one layer of URI percent encoding. Preserves `null`, empty, and blank inputs. |
| [`xml-escaped-to-text`]({{ '/functions/text/encoding/xml-escaped-to-text/' | relative_url }}) | Returns text by decoding XML character data without requiring a containing element. Returns `null` for malformed input and preserves `null`, empty, and blank inputs. |
