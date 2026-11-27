---
description: Take a new idea through the whole SDD workflow (spec, plan, implement, verify), stopping where the developer decides
argument-hint: <idea, issue number, or backlog item>
---
New feature: $ARGUMENTS

Run the steps of the single SDD commands in order. Read each command file and follow it in full; its rules still apply here.
This file only adds the order and the stops. Order: `/spec` -> `/plan` -> `/tasks` -> `/verify` (see the quick start in `specs/README.md`).

1. **Spec**: follow `.claude/commands/spec.md` steps 1-3 (read, ask clarifying questions in rounds, write `spec.md` as `draft`).
   - If the change looks small (typo, comment, docs edit, one-line fix, tooling tweak), ask the developer:
     **short path** (`specs/_templates/small-change.md`, no `plan.md` / `tasks.md`) or **full path**? Use what they choose.
2. **STOP: spec approval.** Show the acceptance criteria and ask for approval (`spec.md` step 4). Wait.
   On approval: status `approved`, date, name. On the short path this approval is also the go-ahead to implement: go to step 5.
3. **Plan** (full path): follow `.claude/commands/plan.md`. If the plan changes the spec, get the spec approved again first.
4. **STOP: go-ahead.** Summarise the plan and ask for the go-ahead to implement. Wait.
5. **Implement**: follow `.claude/commands/tasks.md` (short path: skip writing `tasks.md`). Branch from `main`, status `in progress`.
6. **Verify**: follow `.claude/commands/verify.md` steps 0-4 (criteria results, `specs/system/`, `CHANGELOG.md`, `HANDOFF.md`, status `done`).
   If a required criterion fails, report it and ask how to go on; don't move on as if it passed.
7. **STOP: commit and merge.** Report the results and ask: "Commit and merge into `main`?" Only on yes: commit on the feature branch,
   merge into `main`, delete the merged branch. Then ask separately: "Push?" Push only on yes.

Rules:
- Commit nothing before the question in step 7.
- Never create a release tag (constitution rule 4).
- If the developer says no or "stop" at any point, stop and leave the feature at its current status. It is resumed later
  in plain words ("continue NNN") or with the next single command (see "Resuming" in `specs/README.md`).
- Anything outside the spec's scope goes back to the developer; it may need its own feature folder.
