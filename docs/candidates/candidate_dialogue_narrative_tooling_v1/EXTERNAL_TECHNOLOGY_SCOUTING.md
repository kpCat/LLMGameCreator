# External Technology Scouting - Dialogue Narrative Tooling

Subsystem: dialogue/narrative tooling  
Candidate id: `candidate_dialogue_narrative_tooling_v1`  
Date: 2026-06-29  
Agent: Codex

## Search Scope

- Libraries: Ink, Yarn Spinner core.
- Unity packages: Yarn Spinner for Unity, Ink Unity integration as export/runtime references only.
- Existing .NET packages: upstream project files checked for .NET target compatibility; no package dependency was added.
- Existing repo-local helpers: `DialogueDefinition`, `QuestObjectiveDefinition`, `NarrativeDefinitionValidator`, `PackageAssemblyDialogueQuestsAcceptanceService`, `PackageAssemblyDialogueQuestsAcceptanceTests`, `GeneratorPlanGamePackageAssembler` dialogue/quest mapping, and the package-assembly dialogue/quests product-smoke manifest.
- File formats: `.ink` and `.yarn` as authoring/import/export references only; current candidate does not add parser support.

## External Evidence Checked

- Ink repository: `https://github.com/inkle/ink`
- Ink license: `https://github.com/inkle/ink/blob/master/LICENSE.txt`
- Ink runtime project file: `https://raw.githubusercontent.com/inkle/ink/master/ink-engine-runtime/ink-engine-runtime.csproj`
- Ink releases: `https://github.com/inkle/ink/releases`
- Yarn Spinner core repository: `https://github.com/YarnSpinnerTool/YarnSpinner`
- Yarn Spinner core license: `https://github.com/YarnSpinnerTool/YarnSpinner/blob/main/LICENSE.md`
- Yarn Spinner core project file: `https://raw.githubusercontent.com/YarnSpinnerTool/YarnSpinner/main/YarnSpinner/YarnSpinner.csproj`
- Yarn Spinner core releases: `https://github.com/YarnSpinnerTool/YarnSpinner/releases`
- Yarn Spinner Unity repository: `https://github.com/YarnSpinnerTool/YarnSpinner-Unity`
- Yarn Spinner Unity license: `https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/main/LICENSE.md`
- Yarn Spinner Unity package manifest: `https://raw.githubusercontent.com/YarnSpinnerTool/YarnSpinner-Unity/main/package.json`
- Yarn Spinner Unity releases: `https://github.com/YarnSpinnerTool/YarnSpinner-Unity/releases`

## Candidates Reviewed

| Candidate | Type | License | Runtime dependency? | Offline usable? | Deterministic? | Decision | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Ink | C# narrative scripting language, compiler/runtime | MIT | No for this candidate; future editor-time import/export adapter only | Yes, if used through local compiler/runtime binaries/packages | Mostly deterministic for compiled story state when inputs are fixed; random/external functions would need adapter constraints | `reference_only` | Good fit as reference for branching narrative concepts, knots/stitches/choices, variables and validation diagnostics. Current repo can represent only a bounded subset without schema changes. |
| Yarn Spinner core | C# dialogue compiler/runtime core | MIT | No for this candidate; possible future editor-time adapter only | Yes, core compiler/runtime can be local/offline | Deterministic for fixed source and command/function bindings if all host callbacks are constrained | `adapt_behind_adapter` | Stronger fit for dialogue line/options/commands shape, but direct dependency requires `.csproj` change and adoption review. Candidate recommends designing a replaceable adapter boundary first. |
| Yarn Spinner Unity integration | Unity package/integration | MIT for checked package/repo | No; Unity export reference only | Yes inside Unity project once vendored/installed | Depends on Unity scene/runtime bindings; deterministic only if commands and host state are constrained | `reference_only` | Useful future reference for Unity export/runtime presentation, not Application-layer generator contract. Current candidate must not modify Unity entrypoints. |
| Ink Unity integration | Unity package/integration | MIT per upstream Ink project page/repo signal | No; Unity export reference only | Yes inside Unity project once vendored/installed | Depends on Unity runtime bindings | `defer` | Not required beyond noting that Ink has an official Unity path. No current Unity work is allowed. |

## Accepted/Adapted/Reference/Rejected Decision

- Accepted: none.
- Adapted behind adapter: Yarn Spinner core is the best future adapter candidate if a serial adoption task allows project/dependency changes.
- Used as reference only: Ink, Yarn Spinner Unity integration.
- Rejected: direct dependency in this candidate.
- Deferred: Ink Unity integration and any real `.ink`/`.yarn` parser/compiler invocation.

## Ink Decision

Decision: `reference_only`.

Rationale:

- License: upstream `inkle/ink` is MIT.
- Maintenance: the repository and project files are current enough for evaluation; the public releases page shows a 1.2.0 release line, while the runtime project file currently declares an engine package version of 1.2.1.
- C#/.NET compatibility: the runtime project targets `netstandard1.0` and `netstandard2.0`, which is compatible with modern .NET consumers in principle.
- Unity compatibility: official Unity integration exists, but Unity is not in scope for this candidate.
- Deterministic/offline behavior: suitable only if used from local sources/packages and if host functions/random/external state are constrained.
- LLMGameCreator fit: current package contracts can represent a bounded tree/graph subset as `DialogueDefinition.Nodes`, `DialogueChoiceDefinition`, quest links and sidecar metadata. Full Ink flow features, variables, functions and runtime state cannot be claimed without schema/runtime work.
- Replacement plan: treat Ink import/export as optional adapter implementation behind an internal normalized narrative graph. Do not let `.ink` become the internal source of truth.

## Yarn Spinner Core Decision

Decision: `adapt_behind_adapter`.

Rationale:

- License: upstream `YarnSpinnerTool/YarnSpinner` is MIT.
- Maintenance: the core releases page shows an active 3.x line, and the repo describes itself as the core compiler source.
- C#/.NET compatibility: the checked project file targets `netstandard2.0` and `netstandard2.1` with C# 9, so editor-time use from .NET is plausible.
- Unity compatibility: core is engine-independent; the upstream repo points Unity users to the separate Unity package.
- Deterministic/offline behavior: plausible for compile/parse operations with fixed source and local binaries, but commands/functions must remain declarative and host callbacks must be constrained.
- LLMGameCreator fit: lines, options and commands map more directly to current dialogue choices, quest links, effects and sidecar metadata than Ink's broader story-flow model.
- Scope blocker for direct dependency: adding the package would require `.csproj` changes, which are forbidden for this candidate.
- Replacement plan: define an `IDialogueNarrativeScriptAdapter`-style boundary in a future serial task. The internal contract remains LLMGameCreator-owned, with Yarn imported/exported at the edge.

## Yarn Spinner Unity Integration Decision

Decision: `reference_only`.

Rationale:

- License: checked Unity integration repo/package is MIT.
- Maintenance: the Unity releases page shows a 3.2.x line, and the package manifest reports version `3.2.4`.
- Unity compatibility: package manifest targets Unity `2022.3`; runtime assembly references Unity systems such as TextMeshPro, Addressables, Input System and Localization conditionally/precompiled dependencies.
- LLMGameCreator fit: useful future reference for Unity dialogue presentation/export conventions, but this candidate is Application/docs-only and must not change Unity runtime/build entrypoints.
- Replacement plan: keep Unity-specific behavior out of the generator contract; export normalized dialogue/narrative data that a future Unity projection may consume.

## Adapter Boundary

- LLMGameCreator contract: normalized internal dialogue/narrative records with nodes, choices, quest links, conditions, effects, tags, metadata and preserved source refs.
- Adapter name: future `DialogueNarrativeScriptAdapter` or more specific `YarnDialogueNarrativeAdapter` / `InkDialogueNarrativeAdapter`.
- External dependency boundary: editor-time import/export only; no runtime LLM, RAG, provider, media, network, Unity or Lua execution.
- Replacement plan: adapters produce/consume the same internal contract and diagnostics. Removing one adapter must not change `GamePackage` schema or runtime state contracts.

## Risk Notes

- License/attribution: MIT licenses require license notice preservation if dependencies are adopted. No dependency is adopted by this candidate.
- Runtime footprint: direct runtime dependency is rejected for now. Future adoption should stay editor-time unless explicitly accepted.
- Build impact: direct dependency would require `.csproj` changes and therefore a serial adoption/kernel task.
- Testability: future adapter tests should use tiny fixed `.ink`/`.yarn` fixtures and assert deterministic normalized output plus rejection diagnostics.
- Determinism: external commands, host functions, randomization, time, filesystem and network access must be blocked or represented as unsupported diagnostics.
- Maintenance: Yarn Spinner core appears more active in the current 3.x line; Ink remains mature and stable but should be used as a reference/import-export option, not as the internal model.
- Security: source import must be treated as untrusted content; no execution during validation.
- Paid/proprietary/API dependency: none required for the checked open-source components. Do not rely on paid Unity/asset-store add-ons.

## Conclusion

The candidate should not add Ink or Yarn Spinner as a direct dependency. The correct next serial adoption shape is an LLMGameCreator-owned internal dialogue/narrative contract boundary with optional editor-time adapters. Yarn Spinner core is the stronger future adapter candidate; Ink remains valuable reference material and a possible later import/export adapter. Unity integrations are export/runtime presentation references only.

Final candidate status: `candidate_ready_for_serial_adoption`.
