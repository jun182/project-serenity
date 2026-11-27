---
description: Write tasks.md for a planned feature, then implement it
argument-hint: <NNN or folder name>
---
Feature: $ARGUMENTS (folder in `specs/features/`).

0. Check `spec.md` exists and is `approved`. If not, stop: no work without an approved spec, however small the change.
1. Write `tasks.md` from `specs/_templates/tasks.md`, small steps in order, based on `plan.md` (optional for a small change).
2. Create or switch to the feature branch from `main` (`feature/<name>` or `fix/<name>`), set the spec status to `in progress`.
3. Work through the tasks, ticking them off. Stay inside the spec's scope; anything new goes back to the developer as a spec change.
