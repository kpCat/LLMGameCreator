# Goal 061 — Full Campaign Playable Review Package RC

## Purpose
Consume Goal 060 full-campaign GamePackage materialization matrix evidence and produce a full campaign playable review package release-candidate proof.

The review package must prove that the generator can stage the materialized packages, physical media, preview/export payloads and Unity Alpha command route into a coherent reviewable package for all supported families and seeds.

## Gate

```text
full_campaign_playable_review_package_rc_verification required
```

## Required outcome

A GREEN implementation must prove:

1. Goal 060 is accepted by user handoff before Goal 061.
2. Nine materialized GamePackage JSON files are consumed from Goal 060.
3. Goal 054/055/056/057/058/059 media and Unity proof chain is consumed without fake references.
4. A review package RC manifest is produced.
5. The review package contains or references a deterministic packages folder, media/StreamingAssets payload plan, run scripts, smoke scripts, README, manual checklist and automated proof plan.
6. The Unity Alpha route can select family/seed/package rows and emit deterministic player markers.
7. Automated smoke proves launch and loop markers for the matrix, ideally all 9 rows; if the existing Unity route is too expensive, at minimum all 3 families plus a bounded explanation for remaining seeds must be recorded. Prefer all 9 rows.
8. Save/load/replay evidence is tied to package row ids, not only family ids.
9. Invalid/fake/leak matrix rejects unsafe paths, stale package hashes, fake package ids, missing media, broad Unity mutation claims, provider/LLM/RAG/media-generation claims and nondeterministic row order.
10. Heavy build/log/cache outputs remain ignored unless current repo policy explicitly tracks them.

## Non-goals

- Do not change public GamePackage schema.
- Do not add external dependencies.
- Do not call LLM/provider/RAG/media generators.
- Do not execute arbitrary Lua.
- Do not broaden Runtime or Runtime.Abstractions.
- Do not build a full editor UI.
- Do not create an installer.

## Expected artifact folder

```text
.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/
```

Required compact artifacts:

```text
source-manifest.json
review-package-rc-manifest.json
review-package-file-inventory.json
package-row-selection-matrix.json
unity-player-command-plan.json
unity-player-proof-matrix.json
package-media-binding-audit.json
save-load-replay-package-row-audit.json
manual-review-checklist.md
automated-smoke-script-manifest.json
invalid-review-package-matrix.json
full-campaign-playable-review-package-rc-report.md
```

If real scripts are produced under the review package, include their hashes in the inventory.

## Quality bar

This goal is not a paper-only packaging report. It must stage concrete package/review files and prove consumption through focused tests, product smoke, current-state tests, check-all and artifact-scope guard.
