# Codex task — Goal 054 Media Materialization And Media-Bound Review Package Smoke

## Assignment metadata

Repository:

```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:

```text
C:\Users\endim\LLMGameCreator\
```

Branch:

```text
main
```

Composite goal id/name:

```text
goal_054_media_materialization_review_package
Goal 054: Media Materialization And Media-Bound Review Package Smoke
```

Codex reasoning level:

```text
very high
```

Required gate marker:

```text
media_materialization_review_package_verification required
```

## Status policy

This task must end with a commit and push to `origin/main` regardless of result.

Commit message format:

```text
GREEN Goal 054 media materialization review package
BLOCKED Goal 054 media materialization review package
FAILED Goal 054 media materialization review package
```

Never mark a non-green result as accepted. Never mark `media_materialization_review_package_verification` passed inside this goal.

## Strategic purpose

Goal 053 proved governable media campaign orchestration using deterministic fixture bindings. Goal 054 must make media concrete: physical deterministic media files, hashes, provenance, license decisions, bindings, review package/payload manifests and family smoke proof.

This is not provider integration and not final media generation. It is the bridge from media governance to media-bound generated game review/export payloads.

## Preflight: accept prior gate by user handoff

The user handoff reports Goal 053 GREEN, pushed, check-all green and artifact scope green.

Update the state docs so Goal 053 is accepted by user handoff:

```text
media_asset_campaign_orchestration_verification passed
```

Then start Goal 054 implementation and leave Goal 054 gate as:

```text
media_materialization_review_package_verification required
```

Do not create a separate acceptance-only goal.

## Read-first list

Read first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_054_MEDIA_MATERIALIZATION_UNITY_REVIEW_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_054_MEDIA_MATERIALIZATION_UNITY_REVIEW.md`
8. Goal 053 artifacts under `.llmgc/procedural/goal-053-media-asset-campaign-orchestration/`
9. Goal 047 artifacts under `.llmgc/procedural/goal-047-full-generator-without-media-dry-run/`
10. Goal 040/043 artifacts if needed for preview/export/family source facts.
11. Existing Application-layer evidence/service patterns near:
    - `src/LLMGameCreator.Application/Design/MediaAssetCampaignOrchestration/**`
    - `src/LLMGameCreator.Application/Design/FullGeneratorWithoutMediaDryRun/**`
    - `src/LLMGameCreator.Application/Design/ChunkedRuntimePreviewExportSmoke/**`
12. Existing tests/product smoke patterns for those areas.
13. Existing artifact-scope policy entries for recent goals.

Do not scan the whole repo unless local search shows a precise reason.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_054_MEDIA_MATERIALIZATION_UNITY_REVIEW_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_054_MEDIA_MATERIALIZATION_UNITY_REVIEW.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/agent-tasks/GOAL_054_MEDIA_MATERIALIZATION_UNITY_REVIEW.md
docs/agent-tasks/GOAL_054_LAUNCHER.txt
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/MediaMaterializationReviewPackage/**
tests/LLMGameCreator.Tests/Application/MediaMaterializationReviewPackage/**
tests/LLMGameCreator.Tests/ProductSmoke/MediaMaterializationReviewPackageProductSmokeTests.cs
.llmgc/procedural/goal-054-media-materialization-review-package/**
```

Optional, only if absolutely necessary for a narrow bounded media-aware review bundle proof and only after local inspection shows an existing safe pattern:

```text
src/LLMGameCreator.Application/Design/UnityRuntimeExport/**
src/LLMGameCreator.Application/Design/MinimumPlayableGame/**
```

Do not touch Unity source unless the existing repo pattern clearly supports a narrow media manifest staging proof. If in doubt, keep Goal 054 Application-layer-only.

## Forbidden files / areas

Forbidden unless the user explicitly grants a separate follow-up task:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Also forbidden:

- external media provider calls;
- network calls/downloads;
- ComfyUI/Fooocus/Stability/Freesound/OpenGameArt/Pixabay calls;
- LLM/provider/RAG calls;
- Lua execution or Lua source generation;
- public GamePackage schema changes;
- new NuGet dependencies;
- weakening tests to pass;
- final prose/text content generation masquerading as media proof.

## Exact behavior

### 1. Source loading

Load compact source facts from Goal 053 and Goal 047 evidence. Validate that:

- Goal 053 evidence exists;
- the Goal 053 report records GREEN/produced-for-review and `media_asset_campaign_orchestration_verification required`;
- media request count and binding count are available;
- the three families are present;
- Goal 047 dry-run family facts are present or gracefully recorded as missing diagnostics.

### 2. Materialization queue

Build a deterministic media materialization queue from Goal 053 promoted fixture bindings.

Each queue item must include:

- materialization id;
- family id;
- source request id;
- source binding id;
- media kind: image/audio/etc;
- slot id;
- deterministic output relative path;
- provenance/license status;
- review status;
- expected hash after materialization;
- consumer payload role.

### 3. Deterministic media file materialization

Materialize physical deterministic media files under the Goal 054 artifact folder.

Preferred:

- PNG files for image slots;
- WAV PCM files for audio slots.

BCL-only implementation required. No external package.

PNG writer requirements if implemented:

- valid PNG signature;
- IHDR/IDAT/IEND chunks;
- deterministic pixel buffer derived from family/slot/binding ids;
- deterministic dimensions, for example 32x32 or 64x64;
- CRC validation helper or at least chunk CRC writer;
- stable bytes across runs.

WAV writer requirements if implemented:

- RIFF/WAVE header;
- PCM format;
- deterministic short mono sample sequence derived from family/slot/binding ids;
- stable bytes across runs.

If real PNG/WAV materialization cannot be implemented safely, stop with BLOCKED after preserving diagnostic artifacts. Do not fake GREEN with `.fixture` only.

### 4. Provenance/license ledger

Write a ledger that proves:

- fixture media is repository-generated/deterministic;
- no imported/provider/manual asset is promoted;
- unknown license blocks promotion;
- attribution-required licenses remain review-only unless attribution payload is present;
- generated fixture media has deterministic provenance and hash.

### 5. Binding validation

Validate every promoted media binding:

- source slot exists;
- materialized file exists;
- file hash matches;
- media kind matches slot;
- family has at least one image and one audio fixture;
- no absolute paths;
- no cross-family leakage;
- no unapproved provider/import candidate is bound.

### 6. Media-bound preview/export payloads

Produce media-bound payload records for all three families:

- preview payload id;
- export payload id;
- family id;
- referenced dry-run/scenario id;
- referenced media binding ids;
- physical media file refs;
- hash summary;
- validation status;
- review-package inclusion status.

### 7. Review package / review bundle manifest

Create a deterministic review package manifest under the Goal 054 artifact folder. It does not need to be a runnable Unity package yet, but it must be concrete and file-backed.

Required:

- manifest path list;
- media file list;
- payload list;
- license/provenance list;
- family coverage summary;
- manual review checklist;
- validator summary;
- deterministic hash.

### 8. Invalid/fake/leak matrix

Cover at least these causal scenarios:

- missing Goal 053 source;
- fake media request id;
- fake binding id;
- missing physical media file;
- hash mismatch;
- media kind mismatch;
- unknown/prohibited license promoted;
- imported/provider candidate promoted;
- cross-family binding leak;
- absolute path leak;
- network/provider/LLM/RAG call claim;
- GamePackage schema mutation claim;
- Runtime/UI/Unity mutation claim unless explicitly allowed by optional proof;
- nondeterministic ordering;
- malformed PNG/WAV header;
- missing provenance;
- missing review trace.

Each invalid case must have stable diagnostic codes and expected pass/fail results.

### 9. Evidence artifacts

Write under:

```text
.llmgc/procedural/goal-054-media-materialization-review-package/
```

Required:

```text
source-manifest.json
media-materialization-queue.json
materialized-media-inventory.json
media-provenance-license-ledger.json
media-binding-validation.json
media-review-package-manifest.json
preview-export-media-payloads.json
family-media-smoke-map-panel-rpg.json
family-media-smoke-survival-sandbox.json
family-media-smoke-first-person-grid-dungeon.json
invalid-media-materialization-matrix.json
media-materialization-review-package-report.md
```

Also include physical media files under a deterministic subfolder such as:

```text
.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/**
```

Evidence must be compact, deterministic, no timestamps unless existing deterministic convention requires one, no absolute machine paths, no heavy logs.

### 10. Docs/state update

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Final state:

- Goal 053 accepted by user handoff;
- Goal 054 produced for review;
- `media_materialization_review_package_verification required`;
- next recommended work should move toward media-bound playable/Unity review or real provider adapter selection only after Goal 054 review.

## Tests

Add focused tests in local style. Suggested classes:

```text
MediaMaterializationQueueTests
MediaFixtureWritersTests
MediaLicenseProvenanceLedgerTests
MediaBindingValidationTests
MediaReviewPackageManifestTests
MediaMaterializationInvalidMatrixTests
MediaMaterializationEvidenceTests
MediaMaterializationReviewPackageProductSmokeTests
```

Required proof:

- deterministic queue order;
- physical media files are written;
- PNG/WAV signatures/headers are valid if implemented;
- hash roundtrip stable;
- three families covered;
- every family has image/audio bindings;
- preview/export payload refs resolve to physical files;
- invalid/fake/leak matrix passed;
- evidence artifacts parse;
- no provider/network/LLM/RAG/Lua/GamePackage/Runtime/UI/Unity dependency is required.

## Validation commands

Run from repository root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~MediaMaterializationReviewPackage|FullyQualifiedName~Goal054"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~MediaMaterializationReviewPackageProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal054|FullyQualifiedName~MediaMaterialization"

.\.devflow\scripts\check-all.ps1
```

Then run the existing artifact scope guard using the local repo's established command/pattern. Do not invent a new guard script.

Direct artifact inspection after smoke:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-054-media-materialization-review-package -Recurse -File | Sort-Object FullName | Select-Object FullName,Length
Get-Content .\.llmgc\procedural\goal-054-media-materialization-review-package\media-materialization-review-package-report.md -TotalCount 120
```

Also inspect physical media headers with tests or a small deterministic test helper, not ad-hoc manual claims.

## Pre-authorized bounded repairs

Allowed inside this task:

1. Update stale current-state/handoff guard tests if they hardcode the previous latest gate and block `check-all.ps1`, but keep historical assertions strict.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates old tracked evidence outside Goal 054.
3. Update artifact-scope policy for Goal 054 artifacts.
4. Clean or include task-source files only if they are explicitly part of this Goal 054 pack and within allowed docs paths.

Still forbidden:

```text
git reset
git clean
git stash
git merge
git rebase
git cherry-pick
git push --force
```

## Git policy

You must commit and push final state to `origin/main` even if GREEN/BLOCKED/FAILED.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit changed files>
git diff --stat --cached
git add <explicit allowed paths>
git commit -m "GREEN Goal 054 media materialization review package"
git commit -m "BLOCKED Goal 054 media materialization review package"
git commit -m "FAILED Goal 054 media materialization review package"
git push origin main
```

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Stop/blocked conditions

Commit and push BLOCKED if:

- valid physical media materialization cannot be implemented without external dependency;
- Goal 053 source evidence is missing or inconsistent;
- physical media files cannot be hashed/validated deterministically;
- you need public GamePackage schema change;
- you need Runtime/UI/Unity changes outside optional bounded proof;
- you need network/provider/LLM/RAG/Lua calls;
- check-all fails for an unrelated reason that cannot be bounded-repaired safely;
- artifact scope guard fails and cannot be repaired within allowed policy.

## Final report format

Report in Russian:

```text
Goal 054 выполнен / заблокирован / failed
Status: GREEN / BLOCKED / FAILED
Gate: media_materialization_review_package_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Physical media proof:
<counts, kinds, hashes, PNG/WAV proof status>

Evidence artifacts:
<список>

Проверки:
<команды и результаты>

Invalid/fake/leak matrix:
<кратко>

Bounded repairs:
<если были>

Git:
<commit hash, push result, final status>

Ограничения:
<что не трогалось>

Следующий разумный шаг:
<one paragraph>
```
