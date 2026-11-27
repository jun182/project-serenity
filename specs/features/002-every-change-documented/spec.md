# 002 Every change goes through the workflow

- **Status:** verified
- **Source:** request from NightAnimal333 (chat, 2026-11-27)
- **Approved:** 2026-11-27, NightAnimal333 (chat)
- **Branch:** `feature/every-change-documented`

## Problem
`specs/README.md` lets trivial changes (typos, docs-only edits, one-line fixes, tooling) skip the workflow, so they leave no trail in `specs/features/`.

## Goal
Every change, however small, has a feature folder and goes through the workflow, so the history of what changed and why is complete.

## Scope
- Remove the "when a spec can be skipped" exception. Every change (code, scenes, assets, build settings, tools, docs) gets `specs/features/NNN-name/`.
- Small changes use a short path: one `spec.md` from a new `small-change` template (problem, change, acceptance criteria with results);
  `plan.md` and `tasks.md` are optional for them. Clarify and approval still apply, but can be one quick exchange.
- One shared number sequence for features and fixes.
- Update the constitution, CLAUDE.md, CONTRIBUTING, ONBOARDING and the `/spec`, `/tasks`, `/verify` commands; `/verify` checks the folder exists before merge.

## Out of scope
- Retroactive folders for changes made before 001.

## Acceptance criteria
| ID | Criterion | Check | Result |
|----|-----------|-------|--------|
| AC1 | `specs/README.md` has no skip exception and says every change, including trivial fixes and docs edits, gets a feature folder and goes through the workflow. | review | pass |
| AC2 | `specs/README.md` describes the small-change path and the shared numbering. | review | pass |
| AC3 | `specs/_templates/small-change.md` exists. | `ls` | pass |
| AC4 | Constitution rule 8, CLAUDE.md, CONTRIBUTING and ONBOARDING say every change needs a feature folder. | review | pass |
| AC5 | `/spec` handles small changes; `/tasks` refuses to start without an approved spec; `/verify` checks the folder exists before merge. | review | pass |
| AC6 | No doc still says a change may skip the workflow. | `grep -rni "skip" specs CLAUDE.md CONTRIBUTING.md ONBOARDING.md .claude` | pass (only history in 001) |

## System specs affected
None (process change only).
