# New Version Release Checklist

## Document purpose

This document serves as a pre-release checklist to ensure all documentation, repository files,
and system components are thoroughly verified before a new release.

## README

Review the [README.md](../../README.md) file and verify the following:

1. Update the current project version in `Project versioning` section.
2. Verify that all file paths are correct and up to date.
3. Verify that all URLs to external resources are active and up to date.
4. Review the `Repository structure` section and update it with any new components.
5. Proofread the entire document to ensure the content is factually correct and remains relevant to the new version of the system.

## CHANGELOG

Add a new entry to [CHANGELOG.md](../../CHANGELOG.md) using the following template:

```markdown
## Version MAJOR.MINOR.PATCH

**Release Date:** YYYY-MM-DD 
**License:** LICENSE_TYPE  
**Contributors:**
- [NAME](GITHUB_PROFILE_URL "GitHub profile")
```

## UML Documentation

1. Verify UML diagrams stored in [uml/plant-uml](./uml/plant-uml) directory.
2. Render PlantUML source code to SVG images and place them in in [uml/images](./uml/images) directory.

## Text Documentation

Ensure the following files content is factually correct and remains relevant to the new version of the system:

1. [new-version-release-checklist.md](./new-version-release-checklist.md)
2. [server-api-endpoints.md](./server-api-endpoints.md)

## Server codebase

1. Ensure the development connection string has been replaced with a generic placeholder.
2. Verify the server application binds only to localhost in
[launchSettings.json](../../src/Server/SmartHome.Server.Main/Properties/launchSettings.json).
3. Verify that all client-facing API endpoints have call examples in
[SmartHome.Server.Main.http](../../src/Server/SmartHome.Server.Main/SmartHome.Server.Main.http).
4. Check that the base URL in [SmartHome.Server.Main.http](../../src/Server/SmartHome.Server.Main/SmartHome.Server.Main.http) points to localhost and the port specified in [launchSettings.json](../../src/Server/SmartHome.Server.Main/Properties/launchSettings.json).

## Firmware codebase

1. Ensure no development secrets or credentials were leaked in [secrets.cpp](../../src/Firmware/generic_firmware/secrets.cpp).
