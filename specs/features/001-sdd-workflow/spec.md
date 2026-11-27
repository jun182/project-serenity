# 001 Spec-driven development workflow

- **Status:** done
- **Source:** request from NightAnimal333 (chat, 2026-11-27)
- **Approved:** 2026-11-27, NightAnimal333 (chat)
- **Branch:** `feature/sdd`

## Problem
Project knowledge is spread over CLAUDE.md, ONBOARDING, HANDOFF, BACKLOG, ISSUES, CHANGELOG and MENU-TREE, and some of it is stale.
How the game behaves today is mostly recorded only in code and changelog lines. Changes are made without a written, agreed definition
of "done", so it is hard for a returning developer (or an agent) to know what is intended and what is accidental.

## Goal
Every non-trivial change goes through a light spec-driven workflow: idea -> spec -> plan -> tasks -> implement -> verify -> system specs updated.
The current behaviour of the game is written down once, as the baseline.

## Scope
- `specs/` folder: process README, constitution (rules that must not break), templates, `system/` specs, `features/` folders.
- System specs written from the code on `main` @ 7807e69 (b13). Newer work on `android-port` is out of scope; it is specced when merged.
- Branch model change: work on any feature branch, merge into `main` when ready. `android-port` is no longer the development branch.
- Agent instructions (CLAUDE.md), ONBOARDING, CONTRIBUTING, CHANGELOG header, HANDOFF updated to the new workflow.
- Claude Code project commands: `/spec`, `/plan`, `/tasks`, `/verify`.
- MENU-TREE.md replaced by `specs/system/menus.md`.

## Out of scope
- Code changes. Fixing anything found while writing the system specs (it goes in the open-questions list instead).
- Releasing (tags, BuildInfo bump).
- Moving BACKLOG.md / ISSUES.md content into feature specs (an idea only becomes a spec when someone picks it up).

## Acceptance criteria
| ID | Criterion | Check | Result |
|----|-----------|-------|--------|
| AC1 | `specs/README.md` describes the flow, statuses, folder layout and when a spec can be skipped. | review | pass |
| AC2 | `specs/constitution.md` lists the non-negotiable rules (updater, app id, signing key, hidden dev mode, honest testing, release on owner's OK). | review | pass |
| AC3 | Templates for spec, plan and tasks exist in `specs/_templates/`. | review | pass |
| AC4 | `specs/system/` covers gameplay, controls, menus, dev mode and tunables, settings/persistence, audio and feel, updater, build/test/release; every number in them is taken from code or scene files on `main`. | review against code | pass (values from code, GameplayScene, Obstacle/SerenityBlocker prefabs) |
| AC5 | Differences or doubts found while reading the code are listed as open questions, not silently "fixed" in the specs. | review | pass (10 open questions in specs/system/README.md) |
| AC6 | No doc tells contributors to use `android-port` as the development branch; CONTRIBUTING says feature branch -> `main` when ready. | `grep -rn android-port *.md` | pass (remaining mentions are history only) |
| AC7 | CLAUDE.md requires an approved spec for non-trivial work and updating `specs/system/` before merge. | review | pass |
| AC8 | `.claude/commands/` has `spec`, `plan`, `tasks`, `verify`. | `ls` | pass |
| AC9 | No reference to `MENU-TREE.md` is left (docs and `tools/`). | `grep -rn MENU-TREE` | pass (remaining mentions say it was replaced; nothing points readers to it) |
| AC10 | No game code or scene changed on this branch. | `git diff --stat main -- Assets ProjectSettings` is empty | pass (empty) |
| AC11 | Process, CLAUDE.md and `/spec` say: when a developer requests a feature, the agent asks (many) clarifying questions until the requirements are precise, and records unanswered ones as open questions instead of guessing. | review | pass |

Added 2026-11-27 during implementation (AC11, request from NightAnimal333).
