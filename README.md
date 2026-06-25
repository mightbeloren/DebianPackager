
# DebianPackager

A CLI tool for creating Debian packages.

## Commands

### init

Initialize a new Debian package project.

```bash
DebianPackager init -p <package> -v <version> -a <arch> -m <maintainer> -d <description>
````

### files

Add a file mapping to the package.

```bash
DebianPackager files -s <source> -d <destination>
```

### prebuild

Add a command to run before building.

```bash
DebianPackager prebuild -c <command>
```

### build

Build the Debian package.

```bash
DebianPackager build
```
