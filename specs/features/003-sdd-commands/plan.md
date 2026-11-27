# 003 Plan

## Approach
Docs and one new Claude Code command, no game code.

- **`.claude/commands/create.md`**: a command file in the same style as the others (frontmatter `description`,
  `argument-hint: <idea>`). It doesn't copy the other commands' rules. It tells Claude to read and follow
  `.claude/commands/spec.md`, `plan.md`, `tasks.md` and `verify.md` in order, so there is one source of truth for each step.
  It adds what the single commands don't have: the short/full path question, the explicit stops (spec approval,
  go-ahead after the plan, commit/merge, then push), "commit nothing before the commit/merge question", "never tag", and
  "if the developer says no at a stop, stop and leave the status as it is".
  - Conflict to handle: `/verify` step 5 says "merge only when they say", and `/tasks` doesn't stop after the plan.
    `create.md` puts its stops around those steps, so the single commands stay unchanged (AC6).
  - Rejected: copying the full text of the four commands into `create.md` (they would drift apart over time).
- **`specs/README.md`**: a "Quick start (Claude Code)" section right after the intro. It has a table of the commands in order
  (command, what it does, needs, produces, developer stop), one line for `/create`, and a short "Resuming" paragraph
  (which status or files mean which next step: `draft` -> approve; `approved` without `plan.md` -> `/plan`;
  `approved` with a plan -> `/tasks`; `in progress` -> continue tasks, then `/verify`; `verified` -> system specs and merge).
  The old "Claude Code commands" line at the bottom becomes a pointer to the quick start.
- **`CLAUDE.md`, `ONBOARDING.md`, `CONTRIBUTING.md`**: replace the existing one-line command mentions with the short order
  (`/create <idea>`, or `/spec` -> `/plan` -> `/tasks` -> `/verify`) and a pointer to the quick start.
- **Records**: `CHANGELOG.md` entry, a `HANDOFF.md` line, and the spec's statuses and results.

## Files
- New: `.claude/commands/create.md`, `specs/features/003-sdd-commands/{spec,plan,tasks}.md`.
- Changed: `specs/README.md`, `CLAUDE.md`, `ONBOARDING.md`, `CONTRIBUTING.md`, `CHANGELOG.md`, `HANDOFF.md`.
- Unchanged on purpose: `.claude/commands/{spec,plan,tasks,verify}.md`.

## Constitution check
- Rule 4 (releases): `create.md` says it never tags.
- Rule 8 (specs first): `/create` keeps spec approval before any code, and on the short path that approval is also the go-ahead.
- Updater, signing, dev mode, input, look: not touched.

## Risks
- A new command file may only show up in the `/` menu after Claude Code restarts. If it isn't listed, restart and check again.
- The dry run (AC4) creates a throwaway feature folder. It must be deleted afterwards and must never take the next real number by accident.
  Mitigation: stop the dry run before implementing, delete the folder, and check `git status`.

## Build and test
No build (no code change). AC1, AC2, AC4 and AC5 by review. AC3 with `ls` and the `/` menu. AC4 with a dry run of `/create`
on a throwaway idea, stopped at the go-ahead after the plan. AC6 with `git diff main -- .claude/commands/{spec,plan,tasks,verify}.md`. AC7 with grep.
