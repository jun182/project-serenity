# Spec-driven development (SDD)

**Every change** is written down and agreed **before** it is built, and leaves a trail in `features/`: code, scenes, assets,
build settings, tools and docs, including trivial fixes. There are no exceptions. The specs live here, in the repo, next to the code.

```
specs/
  README.md          this file: the process
  constitution.md    rules no change may break
  _templates/        spec.md, plan.md, tasks.md, small-change.md to copy
  system/            how the game works TODAY (the source of truth for current behaviour)
  features/
    NNN-short-name/  one folder per change: spec.md, plan.md, tasks.md
```

## Quick start (Claude Code)
**One command:** `/create <idea>` runs the whole flow below in order and stops wherever you have to decide.

**Or step by step**, in this order (each is a file in `.claude/commands/`):

| # | Command | What it does | Needs | Produces | You decide |
|---|---------|--------------|-------|----------|------------|
| 1 | `/spec <idea>` | Asks clarifying questions, writes the spec | an idea, issue or backlog item | `features/NNN-name/spec.md` (`draft` -> `approved`) | answer the questions; **approve the spec** |
| 2 | `/plan <NNN>` | Reads the code, writes the plan | spec `approved` | `plan.md` | **go-ahead to implement** |
| 3 | `/tasks <NNN>` | Writes the task list, creates the branch, **implements** | spec `approved` (+ plan) | `tasks.md`, the change, status `in progress` | scope questions, if any |
| 4 | `/verify <NNN>` | Checks every acceptance criterion, updates `system/`, `CHANGELOG.md`, `HANDOFF.md` | the implemented change | results in `spec.md`, status `verified` -> `done` | **commit and merge into `main`?**, then **push?** |

- **Small changes** (typo, comment, docs edit, one-line fix, tooling tweak): you choose the short path (`/spec` with the
  small-change template, then `/tasks`, then `/verify`; no plan or task list) or the full path. `/create` asks you.
- Nothing is committed until you say yes at the end. Release tags only on the owner's OK (constitution rule 4).
- **Resuming** an interrupted feature: say "continue NNN", or run the next command. The next step follows from the spec status:
  `draft` -> approve the spec; `approved` without `plan.md` -> `/plan` (full path) or `/tasks` (short path);
  `approved` with `plan.md` -> `/tasks`; `in progress` -> finish the unticked tasks, then `/verify`;
  `verified` -> update `system/`, `CHANGELOG.md`, `HANDOFF.md`, then commit and merge on your OK.

## The flow
1. **Idea.** From a GitHub issue, `BACKLOG.md`, or a developer explaining what they want.
2. **Clarify.** Before writing anything, ask questions until the requirement is precise. Typical questions:
   what problem it solves, who sees it (player / developer mode only), every input (touch, tilt, pad, keyboard) and both scenes
   (start screen, game), edge cases (paused, dead, won, offline, setting already on), defaults and whether it is remembered
   (PlayerPrefs) or per session, what is explicitly out of scope, and how we will know it works (which check, which device).
   Ask in rounds; don't guess an answer the developer has not given. Unanswered points go into the spec as open questions.
3. **Spec.** Create `features/NNN-short-name/spec.md` from the template (next free number, three digits; features and fixes share
   one sequence). It says *what* and *why*,
   never *how*: problem, goal, scope, out of scope, acceptance criteria. Status `draft`.
4. **Approve.** The developer who asked (or the project owner) reads the spec and accepts the acceptance criteria. Status `approved`,
   with date and name. No code before this.
5. **Plan + tasks.** `plan.md`: which files change, approach, risks, how it will be built and tested. `tasks.md`: a checklist.
   If the plan shows the spec is wrong, change the spec and get it approved again.
6. **Implement** on a feature branch (`feature/<name>` or `fix/<name>`), from `main`. Status `in progress`.
7. **Verify.** Check every acceptance criterion and write the result (pass / fail / not tested + why) in the spec's table.
   Not tested is allowed; claiming a pass that was not checked is not. Status `verified`.
8. **Update the system specs.** Change `system/*.md` so they describe the new behaviour, update `CHANGELOG.md`.
   Status `done`. Then merge the branch into `main`.

## Statuses
`draft` -> `approved` -> `in progress` -> `verified` -> `done` (or `dropped`, with a reason).

## Acceptance criteria
Each criterion is one observable behaviour plus how it is checked. The checks available in this project:
- `menu-check`: `tools/menu-check.sh` (adb key events, `MENU page=... sel=...` log lines)
- `log`: another `adb logcat -s Unity` line (add a `Debug.Log` if needed)
- `screenshot`: `tools/ui-check.sh`
- `editor`: play mode in the Unity editor
- `device`: by hand on a real phone (needed for tilt/gyro, real gamepad, haptics, sound); name the phone
- `review`: reading the code or docs

## Small changes
Nothing skips the workflow, but small changes (a typo, a comment, a docs edit, a one-line fix, a tooling tweak) take a short path:
- one `spec.md` from `_templates/small-change.md`: problem, change, acceptance criteria with results; `plan.md` and `tasks.md` are optional.
- clarify and approve in one quick exchange ("fix X like this, checked by Y: OK?").
- branch `fix/<name>`, verify, update `system/` if behaviour changed, `CHANGELOG.md`, merge.

A change found while working on another feature gets its own folder, unless it is inside that feature's scope.
Updates to `HANDOFF.md` and `CHANGELOG.md` belong to the feature they describe and are committed with it.

## Claude Code commands
`/create <idea>`, or `/spec` -> `/plan` -> `/tasks` -> `/verify`: see the quick start at the top. They live in `.claude/commands/`.
