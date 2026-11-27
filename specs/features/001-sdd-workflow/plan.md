# 001 Plan

## Approach
Docs only. Write the process files first, then the system specs from a full read of `Assets/**/*.cs`, the GameplayScene and the
Obstacle / SerenityBlocker prefabs, then point every existing doc at the new structure.

## Files
- New: `specs/README.md`, `specs/constitution.md`, `specs/_templates/{spec,plan,tasks}.md`,
  `specs/system/{README,gameplay,controls,menus,dev-mode,settings,audio-and-feel,updater,build-test-release}.md`,
  `.claude/commands/{spec,plan,tasks,verify}.md`.
- Changed: `CLAUDE.md`, `CONTRIBUTING.md`, `ONBOARDING.md`, `HANDOFF.md`, `CHANGELOG.md` (header + entry), `tools/menu-check.sh` (comment only).
- Removed: `MENU-TREE.md` (content now in `specs/system/menus.md`; the old b11 findings are history, kept in git).

## Risks
- `android-port` has 10 files of newer work (start screen, PlayerPrefs cache, dead player fix, material leak). The system specs
  describe `main`, so they will be out of date for those areas once that work is merged. Mitigation: noted in `specs/system/README.md`.
- The fork owner (jun182) and their agent use the old branch model; tell them before merging.

## Test
Run the checks in the acceptance table (grep, ls, git diff). No build needed (no code change).
