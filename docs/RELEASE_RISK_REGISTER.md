# Release Risk Register

Status: Goal 097 planning register
Manual gate: `final_roadmap_rebaseline_dream_scope_productivity_verification required`
Accepted: false

## P0 Release Blockers

| Risk | Why it blocks release | Required gate |
|---|---|---|
| Playable quality vs proof quality | Existing evidence can pass while the player experience remains inspection-only or debug-like. | Vertical Slice Final manual checklist with player-visible loop and package export/import proof. |
| Runtime dependency boundary violation | Runtime or Unity must not call LLMs, RAG, media providers, WinForms or external generation tools. | Runtime/player dependency scan and package-only consumption proof. |
| Save/load and infinite world deltas | Large/infinite worlds require bounded, deterministic discovered/mutated state. | Save/load/replay gate for selected finite/infinite world mode. |
| Clean-machine install/export | A local developer proof is not a release proof. | Clean-machine installer/export/player launch smoke. |
| Provider/provenance/licensing | Unlicensed media, map data or provider output can block distribution. | License/provenance manifest and fail-closed export policy. |
| Adult/rating leakage | Adult-capable metadata must not leak into safe/public builds. | Rating export filter and safe fallback gate. |
| Geospatial licensing/ToS/API | Real-world map/geodata ingestion can violate ToS or redistribution rules. | Legal/licensing/provider policy before implementation. |

## P1 Serious Risks

| Risk | Impact | Required mitigation |
|---|---|---|
| Unity performance | Streamed chunks, atlases, UI and save/load may fail target hardware. | Performance budget smoke for selected player target. |
| StreamingAssets/platform issues | Paths, casing, file sizes and platform packaging can diverge across Windows/Unity targets. | Platform handoff probe plus clean export matrix. |
| Visual consistency | Deterministic assets can still look incoherent across packs/styles. | Approved renderer/atlas style review and visual consistency diagnostics. |
| Source-health/code-size | Large files and oversized seams slow review and increase defect risk. | Continue source-health guard and split before source limits are exceeded. |
| Validation noise/duration | Long full checks can hide actionable failures and waste cycles. | Use Goal089 tiers; reserve full/observed full for milestone/release-like work. |
| Runtime vs Unity parity | Headless runtime proof can diverge from Unity/player behavior. | Shared package inputs plus side-by-side runtime/player proof for milestone gates. |

## P2 Technical Debt

| Risk | Impact | Required mitigation |
|---|---|---|
| Repeated proof helpers | Repeated hash/read/evidence code can drift. | Extract only when duplication causes real maintenance or behavior risk. |
| Large method/file debt | Known debt remains in Application and Unity bootstrap areas. | Dedicated bounded decomposition goals with tests held fixed. |
| Artifact timestamp-like values | Deterministic claims become harder to audit. | Normalize volatile values in future reproducibility pass. |
| Visual final renderer missing | Text SVG proof is not production output. | Renderer/atlas prototype and approved output contract. |
| Provider quarantine missing | Future provider outputs cannot be safely promoted. | Candidate quarantine/provenance/review ledger gate. |
| Release documentation gaps | Users need supported/unsupported mode clarity. | v1 docs and sample package pass. |

## P3 Deferrable Polish

| Risk | Impact | Defer rule |
|---|---|---|
| Dashboard/UI polish | Functional review surfaces may remain dense. | Defer until core player/export loop is stable. |
| Extra visual styles | More styles can distract from one coherent release path. | Add only when a milestone needs them. |
| Advanced dream tracks | Realism/geospatial/space-rangers-like tracks can explode scope. | Keep as future register until v1 path is stronger. |
| Broad refactors | Cleanup can consume limit without player value. | Only refactor to remove a named blocker or source-health breach. |

## Release Gate Plan

1. Vertical Slice Final: prove one generated player-facing loop plus export/import and risk review.
2. Strong Alpha: prove repeatable multi-family generation, Unity/player path, save/load deltas and rating-safe export behavior.
3. v1 Full Final: prove clean-machine install/export/player launch, docs, samples, diagnostics, dependency/license audit and release validation.
4. Dream Full Final: only after v1, select specific dream tracks and require research/legal gates before implementation.
