# CODEX GOAL - Goal 014 Unity Playable Presentation And Firewall-Safe Build

## Command

Run this file with:

```text
/goal
```

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/GOAL_014_UNITY_PLAYABLE_PRESENTATION_AND_FIREWALL_SAFE_BUILD.md`
6. accepted Goal 013 Alpha build/play-loop seams only where needed for selected package/runtime/build evidence;
7. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
8. `unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs`
9. focused Alpha build tests and smoke route directly needed for regression compatibility;
10. existing package/runtime definitions directly required by selected loop validation.

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Start only because the user/assistant explicitly confirms:

```text
alpha_runnable_windows_build_verification passed
```

Goal 014 may create S114-S121 and must stop at:

```text
unity_playable_presentation_firewall_safe_build_verification
```

Do not create S122, Goal 015 or post-Goal-014 work.

## Execute

Implement exactly:

```text
docs/GOAL_014_UNITY_PLAYABLE_PRESENTATION_AND_FIREWALL_SAFE_BUILD.md
```

## Allowed Files

Primary allowed areas:

- `docs/GOAL_014_UNITY_PLAYABLE_PRESENTATION_AND_FIREWALL_SAFE_BUILD.md`
- this wrapper
- a narrow new area under `src/LLMGameCreator.Application/Design/UnityPlayableAlpha/` or `src/LLMGameCreator.Application/Design/UnityPresentation/`
- existing `src/LLMGameCreator.Application/Design/AlphaBuild/` files only when directly reusing accepted Goal 013 evidence
- focused tests under `tests/LLMGameCreator.Tests/Application/UnityPlayableAlpha/` or `tests/LLMGameCreator.Tests/Application/UnityPresentation/`
- one product smoke file under `tests/LLMGameCreator.Tests/ProductSmoke/`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/`
- `unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs`
- `unity/LLMGameCreatorAlpha/Packages/manifest.json` only if strictly required and justified
- `unity/LLMGameCreatorAlpha/ProjectSettings/*` only if strictly required and justified
- `.devflow/scripts/run-product-smoke.ps1`
- `.llmgc/procedural/unity-playable-alpha/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test/build failure proves it necessary:

- the narrow existing Unity runtime export/materialization seam;
- the narrow existing package/runtime validation seam;
- one compact sample under `samples/unity-playable-alpha/`.

Do not edit any other file without reporting a blocker.

Do not edit `.sln` or C# `.csproj` files.

## Non-Negotiable Execution Shape

- Build on accepted Goal 013 evidence.
- Produce a visible Unity player, not only logs.
- Prove movement and interaction through automated player logs.
- Keep generated data as package/config/assets; do not hardcode the game by style id.
- Build normal release-style Windows player, not Development/Profiler/Debug player.
- Do not add broad Windows Firewall rules.
- Product smoke must validate the new artifacts.
- Invalid expectations never determine actual validity.
- One final gate only.

## Firewall Note

The user reports repeated Windows Firewall prompts for Unity EXEs. Treat this as a build discipline issue for our produced player:

- remove Unity development/profiler/debug networking options;
- keep build output stable;
- verify build entrypoint does not use profiler/debug flags;
- do not use Windows Firewall rules as the primary fix.

If a firewall prompt persists, report exact conditions and keep the final gate required.

## Verification

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityPlayableAlpha|FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-playable-alpha
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for mojibake, machine-specific nondeterminism and exact `S122|Goal 015|goal_015` markers, excluding Goal/task prohibition text.

## Stop Conditions

Stop instead of weakening acceptance when this requires public schema redesign, broad Unity rewrite, WinForms/UI edits, Runtime Preview proof, external provider/media/LLM/RAG/Lua/generator-library execution, broad firewall allow rules, fake play evidence, or `.sln`/C# `.csproj` edits.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S122 or Goal 015.
- No Runtime Preview dependency as Unity playable proof.
- No fake Windows executable, Unity build, movement, interaction or firewall-safe claim.
- No broad Windows Firewall allow rules.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No WinForms/UI edits.
- No Visual Studio `.sln` or C# `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report every item required by the primary Goal document, then stop at the single final gate without marking it passed.
