# Final report format

Return this structure:

- Final status: GREEN / BLOCKED / FAILED.
- Latest commit before work.
- Latest commit after work.
- Push status to origin/main.
- Preflight summary:
  - branch;
  - origin/main match;
  - Goal 080 GREEN/accepted=false/handoff status;
  - Goal 080 commit-message-order P3 note;
  - forbidden baseline.
- Changed files grouped by area.
- Implemented behavior.
- Proof metrics:
  - consumed Goal 080 report/proof/package hashes;
  - playthrough command count;
  - transcript step count;
  - package read proof;
  - state hash chain before/after/replay;
  - coverage for rows/targets/actions;
  - negative-proof scenarios and rejection statuses;
  - report hash.
- Quality gate:
  - max C# line length;
  - raw LF/CR source checks;
  - minified source count;
  - files over 1000 lines;
  - parent workspace line count;
  - AlphaRuntimeBootstrap line count/hash unchanged;
  - forbidden areas touched yes/no.
- Validation results with pass/fail counts.
- Artifact-scope result.
- Evidence hygiene result.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.
- Goal usage if available.
