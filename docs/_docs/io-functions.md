---
title: IO functions
subtitle: Functions applicable to path and file values
tags: [functions, io]
keywords: [creation-datetime, creation-datetime-utc, directory, extension, filename, filename-without-extension, root, size, update-datetime, update-datetime-utc] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### creation-datetime

###### Aliases: `file-to-creation-datetime`, `file-to-creation-dateTime`

###### Overview

Returns the creation time of the file provided as argument in local time.

##### creation-datetime-utc

###### Aliases: `file-to-creation-datetime-utc`, `file-to-creation-dateTime-utc`

###### Overview

Returns the creation time of the file provided as argument in UTC.

##### directory

###### Alias: `path-to-directory`

###### Overview

Returns the directory information of a file path provided as argument. The value is always ending by `/` character. Returns `empty` if path does not contain root directory information or is `null`.

##### extension

###### Alias: `path-to-extension`

###### Overview

Returns the extension of a file path provided as argument.

##### filename

###### Alias: `path-to-filename`

###### Overview

Returns the file name and extension of a file path provided as argument.

##### filename-without-extension

###### Alias: `path-to-filename-without-extension`

###### Overview

Returns the file name without the extension of a file path provided as argument.

##### root

###### Alias: `path-to-root`

###### Overview

Returns the root directory information of a file path provided as argument. Returns `empty` if path does not contain root directory information or is `null`.

##### size

###### Alias: `file-to-size`

###### Overview

Returns the size of the file provided as argument in bytes.

##### update-datetime

###### Aliases: `file-to-update-datetime`, `file-to-update-dateTime`

###### Overview

Returns the last update time of the file provided as argument in local time.

##### update-datetime-utc

###### Aliases: `file-to-update-datetime-utc`, `file-to-update-dateTime-utc`

###### Overview

Returns the last update time of the file provided as argument in UTC.

<!-- END AUTO-GENERATED -->
