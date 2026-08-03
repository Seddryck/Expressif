---
title: Using Expressif with Docker
tags: [docker, CLI]
keywords: [docker, CLI]
---
Expressif can be built and executed as a Docker image. This allows you to use the Expressif CLI without installing Expressif or the .NET runtime directly on your machine.

The general workflow is:

```text
Download and extract the container archive
        ↓
Build an Ubuntu or Alpine image
        ↓
Run Expressif commands through Docker
```

## Container archive

The Expressif container archive contains the files required to build Docker images locally.

Its content is similar to:

```text
build-container.ps1
ubuntu/
└── Dockerfile
alpine/
└── Dockerfile
```

The archive contains:

* a PowerShell script named `build-container.ps1`;
* a Dockerfile for Ubuntu;
* a Dockerfile for Alpine.

The archive does **not** contain prebuilt Docker images.

Instead, it contains the manifests required to build images containing the latest available version of Expressif.

## How the Expressif version is selected

The Dockerfiles do not target the Expressif version associated with the container archive itself.

When you build an image, the Dockerfile looks up and downloads the latest available Expressif release at that moment.

This means that the Expressif version is selected at **image build time**.

```text
Run build-container.ps1
        ↓
The latest Expressif release is resolved
        ↓
Expressif is downloaded into the image
        ↓
The resulting image contains that fixed version
```

You do not necessarily need to download a newer container archive when a new Expressif version is released.

Re-running `build-container.ps1` is sufficient to retrieve the latest Expressif version, provided that the existing Dockerfiles remain compatible with the current Expressif release format.

## Expressif versions are fixed inside built images

Once an image has been built, the version of Expressif contained in that image is fixed.

Running the image does not check whether a newer Expressif release exists.

Starting a new container from the same image also does not update Expressif. Every container created from that image uses the same embedded version.

To use a newer Expressif version, rebuild the image by running `build-container.ps1` again.

```text
Existing image
    └── Expressif version remains unchanged

Run build-container.ps1 again
    └── A new image is built with the latest available version
```

## Prerequisites

Before building the images, ensure that Docker and PowerShell are available.

You can verify Docker with:

```powershell
docker --version
```

You can verify PowerShell with:

```powershell
pwsh --version
```

On Windows, Docker Desktop must be running before executing the build script.

## Extract the archive

Extract the container archive into a directory of your choice.

For example:

```text
Expressif-containers/
├── build-container.ps1
├── ubuntu/
│   └── Dockerfile
└── alpine/
    └── Dockerfile
```

Open PowerShell and move into the extracted directory:

```powershell
Set-Location .\Expressif-containers
```

## Build the Ubuntu image

Run:

```powershell
.\build-container.ps1 ubuntu
```

The script builds an Ubuntu-based Docker image and tags it as:

```text
expressif:ubuntu
```

The build downloads the latest available Expressif release and installs it into the image.

## Build the Alpine image

Run:

```powershell
.\build-container.ps1 alpine
```

The script builds an Alpine-based Docker image and tags it as:

```text
expressif:alpine
```

Alpine images are generally smaller, while Ubuntu images may be easier to inspect or troubleshoot and may offer broader compatibility with native dependencies.

For normal Expressif CLI usage, either variant can be used.

## Verify the built images

List the available Expressif images with:

```powershell
docker image ls expressif
```

The output should contain one or both of the following tags:

```text
expressif:ubuntu
expressif:alpine
```

You can verify the Expressif version embedded in the Ubuntu image with:

```powershell
docker run --rm expressif:ubuntu version
```

For Alpine:

```powershell
docker run --rm expressif:alpine version
```

## Run Expressif

Arguments placed after the image name are passed directly to the Expressif CLI.

The general syntax is:

```powershell
docker run --rm expressif:<variant> <command> [arguments]
```

For example:

```powershell
docker run --rm expressif:ubuntu version
```

```powershell
docker run --rm expressif:ubuntu evaluate "absolute | add(3)" input "-5"
```

```powershell
docker run --rm expressif:ubuntu validate "absolute | add(3)" input "-5"
```

The same commands can be run with the Alpine image:

```powershell
docker run --rm expressif:alpine evaluate "absolute | add(3)" input "-5"
```

## Display the Expressif help

Run:

```powershell
docker run --rm expressif:ubuntu --help
```

You can also display help for a specific command:

```powershell
docker run --rm expressif:ubuntu evaluate --help
```

## What happens when no command is provided

Running the image without an Expressif command:

```powershell
docker run --rm expressif:ubuntu
```

starts the Expressif CLI, but no command is passed to it.

Expressif therefore displays its usage information and reports that a command is required.

For example:

```text
Required command was not provided.

Description:
  Evaluate and validate Expressif expressions.

Usage:
  Expressif.Cli [command] [options]
```

This does not indicate a problem with the image. It confirms that the Expressif executable was started successfully.

## Container lifecycle

Expressif is a command-line application, not a continuously running service.

When you execute:

```powershell
docker run --rm expressif:ubuntu version
```

Docker performs the following operations:

1. A new container is created from the `expressif:ubuntu` image.
2. The Expressif `version` command is executed.
3. The command produces its output.
4. The container stops.
5. The stopped container is deleted because `--rm` was specified.

The Docker image itself is not deleted. It remains available for subsequent commands.

Each new `docker run` command creates a new short-lived container from the same image.

## Purpose of `--rm`

The `--rm` option tells Docker to delete the container after the Expressif command completes.

For example:

```powershell
docker run --rm expressif:ubuntu version
```

Without `--rm`:

```powershell
docker run expressif:ubuntu version
```

the container still stops when the command completes, but the stopped container remains stored by Docker.

Stopped containers can be listed with:

```powershell
docker ps -a
```

Because Expressif commands are normally stateless, using `--rm` is recommended.

## Rebuild an image to update Expressif

To update the Ubuntu image to the latest available Expressif release, run:

```powershell
.\build-container.ps1 ubuntu
```

To update the Alpine image:

```powershell
.\build-container.ps1 alpine
```

The build script forces Docker to rebuild the image and retrieve the latest Expressif release instead of reusing a previously downloaded release.

After rebuilding, confirm the embedded version with:

```powershell
docker run --rm expressif:ubuntu version
```

You do not need to download a newer container archive for every Expressif release.

A newer archive is only required when the container definitions or build process themselves have changed, or when the existing Dockerfiles are no longer compatible with the Expressif release format.

## Image tags and rebuilds

The build script uses a stable tag for each variant:

```text
expressif:ubuntu
expressif:alpine
```

When the image is rebuilt, the tag is assigned to the newly built image.

Consequently:

```powershell
docker run --rm expressif:ubuntu version
```

uses the most recently built Ubuntu image carrying that tag.

Existing containers created from an older image are not updated. However, this is normally irrelevant when using `--rm`, because those containers are removed immediately after execution.

## Choosing between Ubuntu and Alpine

Use Ubuntu when:

* you prefer a familiar Linux environment;
* you want easier interactive troubleshooting;
* you expect to add tools or native dependencies to the image.

Use Alpine when:

* image size is a priority;
* you want a minimal runtime environment;
* your usage is limited to running the Expressif CLI.

Both images expose the same Expressif commands.

## Complete example

The following example extracts the archive, builds the Ubuntu image, verifies its version, and evaluates an expression:

```powershell
Expand-Archive `
    -Path .\Expressif-containers.zip `
    -DestinationPath .\Expressif-containers

Set-Location .\Expressif-containers

.\build-container.ps1 ubuntu

docker run --rm expressif:ubuntu version

docker run --rm expressif:ubuntu evaluate "absolute | add(3)" input "-5"
```

To use Alpine instead:

```powershell
.\build-container.ps1 alpine

docker run --rm expressif:alpine version

docker run --rm expressif:alpine evaluate "absolute | add(3)" input "-5"
```

## Troubleshooting

### Docker is not running

If Docker cannot connect to the Docker daemon, ensure that Docker Desktop or Docker Engine is running.

You can test Docker with:

```powershell
docker info
```

### The build script cannot be executed

PowerShell may block scripts downloaded from the internet.

You can unblock the script with:

```powershell
Unblock-File .\build-container.ps1
```

Then run it again:

```powershell
.\build-container.ps1 ubuntu
```

### The image cannot be found

An error such as:

```text
Unable to find image 'expressif:ubuntu' locally
```

usually means that the image has not yet been built or that the expected tag was not created.

Build it with:

```powershell
.\build-container.ps1 ubuntu
```

Then verify it with:

```powershell
docker image ls expressif
```

### The image still contains an older Expressif version

Rebuild the image:

```powershell
.\build-container.ps1 ubuntu
```

Then check the version again:

```powershell
docker run --rm expressif:ubuntu version
```

Also verify that the existing container definition is still compatible with the current structure of Expressif GitHub releases.

### No Expressif command was provided

This command:

```powershell
docker run --rm expressif:ubuntu
```

starts Expressif without arguments.

Provide a command such as:

```powershell
docker run --rm expressif:ubuntu version
```

or:

```powershell
docker run --rm expressif:ubuntu evaluate "absolute | add(3)" input "-5"
```
