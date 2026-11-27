# 003 Command list and /create

- **Status:** done
- **Source:** request from NightAnimal333 (chat, 2026-11-27)
- **Approved:** 2026-11-27, NightAnimal333 (chat)
- **Branch:** `feature/sdd-commands`

## Problem
The SDD commands (`/spec`, `/plan`, `/tasks`, `/verify`) are mentioned in one line at the bottom of `specs/README.md` and
in passing elsewhere. A new developer using Claude Code can't see which commands are required, in what order, what each one
does (e.g. that `/tasks` also implements), or where they have to step in. There is also no single command that takes
an idea through the whole workflow.

## Goal
Anyone using Claude Code can read one short list and start working with SDD right away, and can run `/create <idea>` to
go through the whole workflow in order, stopping only where the developer has to decide.

## Scope
- **Command list.** A clear, ordered list of the SDD commands: for each one, what it does, what it needs first
  (e.g. an approved spec), what it produces, and where the developer has to approve or decide. It also says how to
  resume an interrupted feature and how `/create` relates to the single commands. The list goes in:
  - `specs/README.md` (a quick-start section near the top, the full reference),
  - `CLAUDE.md` (short version),
  - `ONBOARDING.md` and `CONTRIBUTING.md` (short version or a pointer to the list).
- **`/create <idea>`.** A new command that runs the steps of `/spec`, `/plan`, `/tasks` and `/verify` in order for a
  **new** feature, with these stops:
  1. Clarify: asks questions, like `/spec`, until the requirement is precise.
  2. If the change looks small (typo, comment, docs edit, one-line fix, tooling tweak), asks the developer whether
     they want the **short path** (small-change template, no plan.md/tasks.md) or the **full path**.
  3. Writes the spec (status `draft`). **Stop:** the developer approves the spec (status `approved`, date, name).
  4. Writes the plan (full path only). **Stop:** the developer gives the go-ahead to implement.
  5. Writes tasks (full path), creates the feature branch from `main`, implements (status `in progress`).
  6. Verifies every acceptance criterion and updates `specs/system/`, `CHANGELOG.md` and `HANDOFF.md` (status `verified`, then `done`).
  7. **Stop:** asks whether to commit and merge into `main`. Only on yes: commit and merge. Pushing is a separate question.
  - Short path: steps 4 and the tasks part of 5 are skipped. The spec approval in step 3 is also the go-ahead to implement.
  - Nothing is committed before the question in step 7.
  - Any rule of the existing commands still applies inside `/create` (e.g. a spec change found during planning goes
    back to the developer for approval; anything outside the spec's scope goes back to the developer).
- **Resuming.** There is no `/continue`. An interrupted feature is resumed by asking in plain words ("continue 003")
  or by running the next command from the list. The docs explain how Claude finds the next step from the spec status
  and the files in the feature folder.

## Out of scope
- Changing `/spec`, `/plan`, `/tasks` or `/verify`. They keep their current behaviour; `/create` runs their steps in order.
- A `/continue` command.
- Release tagging. `/create` never tags (constitution rule 4).
- Automation outside Claude Code (hooks, CI).

## Details
Developer-facing only. No game, scene or input changes. If the developer says no at a stop, `/create` stops and
leaves the feature at its current status; it can be resumed later as described above.

## Acceptance criteria
| ID | Criterion | Check | Result |
|----|-----------|-------|--------|
| AC1 | `specs/README.md` has a quick-start section near the top: the ordered command list, what each command does and needs, where the developer approves, and how to resume. | review | pass |
| AC2 | `CLAUDE.md`, `ONBOARDING.md` and `CONTRIBUTING.md` give the command order or point to the list in `specs/README.md`, and mention `/create`. | review | pass |
| AC3 | `.claude/commands/create.md` exists, and `/create` appears in Claude Code's command list. | `ls` + Claude Code `/` menu | pass (file exists; `create` registered in Claude Code in this session without restart) |
| AC4 | `/create` runs the steps in order and stops at: clarifying questions, the small/full path question (small changes only), spec approval, the go-ahead after the plan (full path), and the commit/merge question. Pushing is asked separately. | review of `create.md` + a dry run on a throwaway idea (stopped before implementing, folder deleted afterwards) | pass (review; dry run 2026-11-27 "blue start buttons": stopped at questions, short/full path, spec approval, re-approval after the plan changed the spec, go-ahead; nothing committed; folder deleted. Steps after the go-ahead not exercised) |
| AC5 | `/create` commits nothing before the commit/merge question and never tags a release. | review of `create.md` | pass |
| AC6 | `/spec`, `/plan`, `/tasks` and `/verify` are unchanged. | `git diff main -- .claude/commands/{spec,plan,tasks,verify}.md` is empty | pass (empty diff) |
| AC7 | No doc mentions a `/continue` command. | `grep -rn "/continue" specs CLAUDE.md CONTRIBUTING.md ONBOARDING.md .claude` (only this spec) | pass (only this spec) |

## Open questions
- None. (Branch base: 002 merged into `main` first, then `feature/sdd-commands` from `main`; approved 2026-11-27.)

## System specs affected
None (process change only).
