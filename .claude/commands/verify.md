---
description: Check a feature's acceptance criteria and update the system specs
argument-hint: <NNN or folder name>
---
Feature: $ARGUMENTS (folder in `specs/features/`).

1. Build if code changed (`specs/system/build-test-release.md`), check for `error CS`.
2. Check every acceptance criterion with the method in its Check column. Write the result in the table: pass, fail, or
   not tested + why (e.g. needs a real gamepad). Never mark pass without having checked it.
3. If all required criteria pass (or the developer accepts the untested ones): status `verified`.
4. Update `specs/system/*.md` so they describe the new behaviour, add a `CHANGELOG.md` entry, update `HANDOFF.md`. Status `done`.
5. Report the results to the developer. Merge into `main` only when they say the work is ready.
