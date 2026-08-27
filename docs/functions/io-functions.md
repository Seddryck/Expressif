---
layout: docs
title: "Io functions"
parent: "Functions library"

nav_order: 50
has_children: true
has_toc: false
permalink: /functions/io-functions/
tags:
  - functions
  - io

generated: true
---

Reference documentation for Expressif functions in the `io` scope.

| Name | Overview |
|:-----|:---------|
| [`creation-datetime`]({{ '/functions/io/creation-datetime/' | relative_url }}) | Returns the creation time of the file provided as argument in local time. |
| [`creation-datetime-utc`]({{ '/functions/io/creation-datetime-utc/' | relative_url }}) | Returns the creation time of the file provided as argument in UTC. |
| [`directory`]({{ '/functions/io/directory/' | relative_url }}) | Returns the directory information of a file path provided as argument. The value is always ending by `/` character. Returns `empty` if path does not contain root directory information or is `null`. |
| [`extension`]({{ '/functions/io/extension/' | relative_url }}) | Returns the extension of a file path provided as argument. |
| [`filename`]({{ '/functions/io/filename/' | relative_url }}) | Returns the file name and extension of a file path provided as argument. |
| [`filename-without-extension`]({{ '/functions/io/filename-without-extension/' | relative_url }}) | Returns the file name without the extension of a file path provided as argument. |
| [`root`]({{ '/functions/io/root/' | relative_url }}) | Returns the root directory information of a file path provided as argument. Returns `empty` if path does not contain root directory information or is `null`. |
| [`size`]({{ '/functions/io/size/' | relative_url }}) | Returns the size of the file provided as argument in bytes. |
| [`update-datetime`]({{ '/functions/io/update-datetime/' | relative_url }}) | Returns the last update time of the file provided as argument in local time. |
| [`update-datetime-utc`]({{ '/functions/io/update-datetime-utc/' | relative_url }}) | Returns the last update time of the file provided as argument in UTC. |
