# Final report format

Return this exact structure:

```text
Final status: GREEN / BLOCKED / FAILED
Latest commit before work:
Latest commit after work:
Pushed to origin/main: yes/no

Preflight:
- Current branch:
- Current main top commit:
- Goal 075 handoff recorded: yes/no
- c8343e8 docs adaptive quality present: yes/no
- c8343e8 handling: integrated/debt/no action, with reason

Changed files:
- ...

Implemented behavior:
- Application seam:
- WinForms workspace:
- GamePackage/materialization handoff:
- Unity/player/staged artifact proof:

Proof:
- Before hash:
- After hash:
- Rollback/replay proof:
- Staged artifact proof:
- Tamper/missing artifact negative proof:

Quality gate:
- max C# line length:
- minifiedSourceFileCount:
- filesOver1000LinesCount:
- AlphaRuntimeBootstrap.cs line count before/after or inspected/no-change:
- forbidden areas touched: yes/no
- absolute paths/timestamps/heavy logs in tracked evidence: yes/no

Validation:
- restore:
- build:
- Goal 075 focused tests:
- Goal 076 focused tests:
- Goal 076 product smoke:
- CurrentState tests:
- check-all:
- artifact scope:

Goal 076 artifacts:
- list required artifacts and whether present
- final report hash

Remaining P2/P3 debt:
- ...

Final git status:
- ...
```
