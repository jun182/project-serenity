---
description: Turn an idea into a feature spec (asks clarifying questions first)
argument-hint: <idea, issue number, or backlog item>
---
Follow `specs/README.md` (steps 1-4) for this idea: $ARGUMENTS

1. Read `specs/constitution.md` and the `specs/system/` files the idea touches. If it is a GitHub issue, read it (`gh issue view`).
2. **Clarify before writing.** Ask the developer questions until the requirement is precise. Ask in rounds, a few at a time,
   most important first. Cover: the problem and who it is for (players / developer mode), each scene (start screen, game),
   each input (touch, tilt, pad, keyboard), edge cases (paused, dead, won, offline, setting already on), defaults and persistence
   (PlayerPrefs or session-only), what is out of scope, and how each requirement will be checked (menu-check, log, screenshot,
   editor, which device). Point out conflicts with the constitution or with current behaviour. Never fill a gap with a guess;
   if something stays unanswered, put it under Open questions.
   For a small change (typo, comment, docs edit, one-line fix, tooling tweak) one short round is enough, but still ask.
3. Pick the next free number in `specs/features/` (three digits, one sequence for features and fixes) and write
   `specs/features/NNN-short-name/spec.md` from `specs/_templates/spec.md` (small change: `specs/_templates/small-change.md`),
   status `draft`. What and why only, no implementation. Every change gets a folder; nothing skips this.
4. Show the acceptance criteria to the developer and ask for approval. Only after they approve: set status `approved` with date and name.
   Do not write code.
