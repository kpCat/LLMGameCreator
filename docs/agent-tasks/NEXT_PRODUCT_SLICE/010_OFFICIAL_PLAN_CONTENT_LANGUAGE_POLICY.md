# Product Slice 010 Task: Official Product Plan + Content Language Policy Foundation

## Task type

Bounded foundation slice.

## Goal

Add official repository-level plan docs and implement the first content language policy foundation for generated game content.

The next generated games should not accidentally default to English when the project/game wants Russian.

## Recommended Codex reasoning level

High.

Reason:
This touches product documentation, generation UI/service path, prompt/request construction, validation diagnostics, tests, and smoke coverage. Do not use Max/Ultra unless repair is needed.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
docs/CAPABILITY_GRAPH_AND_GENERATOR_CATALOG_PLAN.md
docs/PRODUCT_SLICE_010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY.md

src/LLMGameCreator.WinForms/Pages/**/Strict*Artifacts*.cs
src/LLMGameCreator.WinForms/Pages/**/Llm*Artifacts*.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/**
src/LLMGameCreator.Application/Validation/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
```

Then search narrowly for:
- strict LLM artifact request/prompt construction;
- batch preset request path;
- current project folder/settings persistence patterns;
- generated artifact validation diagnostics.

## Allowed files

```text
docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
docs/CAPABILITY_GRAPH_AND_GENERATOR_CATALOG_PLAN.md
docs/PRODUCT_SLICE_010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json

src/LLMGameCreator.Application/Design/GeneratorPlans/**
src/LLMGameCreator.Application/Validation/**
src/LLMGameCreator.Application/Projects/**
src/LLMGameCreator.WinForms/Pages/**
src/LLMGameCreator.WinForms/CompositionRoot.cs

tests/LLMGameCreator.Tests/Application/**
tests/LLMGameCreator.Tests/ProductSmoke/**
tests/LLMGameCreator.Tests/WinForms/**

.devflow/scripts/run-product-smoke.ps1
.devflow/CURRENT_RUN.md
```

Only touch `CompositionRoot.cs` if registering new service/options is needed.

## Forbidden files

```text
LLMGameCreator.sln
*.csproj
generator-library/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages.
Do not call real LLM/provider in tests.
Do not implement translation.
Do not rewrite existing artifacts.
Do not break existing English fixtures/smoke data.

## Required behavior

### 1. Official docs

Add/keep these docs:

```text
docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
docs/CAPABILITY_GRAPH_AND_GENERATOR_CATALOG_PLAN.md
docs/PRODUCT_SLICE_010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY.md
```

If docs already exist from this pack, keep them and improve only if needed.

### 2. Content language policy model

Add a model/service for content language policy.

Possible names:

```text
ContentLanguagePolicy
ContentLanguagePolicyService
ContentLanguageCode
ContentLanguagePromptInstructionProvider
```

Minimum supported codes:

```text
ru
uk
en
```

Default for new generation UI should be `ru`, unless an existing project setting says otherwise.

Technical ids remain ASCII/kebab_case regardless of content language.

### 3. Persist policy per project if project context is available

Preferred path:

```text
.llmgc/settings/content-language-policy.json
```

Example:

```json
{
  "contentLanguage": "ru",
  "fallbackContentLanguage": "en",
  "technicalIdPolicy": "ascii_kebab_case"
}
```

If current project folder is unavailable in a UI context, use in-memory default and report it in UI/status.

### 4. LLM Artifacts UI integration

Add a small dropdown/selector on the LLM Artifacts/batch generation page:

```text
Content language: Russian / Ukrainian / English
```

Default: Russian.

Changing it should affect future generation requests, not rewrite existing artifacts.

Do not overbuild full app localization.

### 5. Prompt/request integration

Where strict LLM artifact prompts/requests are built, append/inject content language instruction.

For `ru`:

```text
Generate all player-facing game content in Russian.
Keep technical ids in ASCII/kebab_case.
Do not translate ids.
Do not output English prose unless it is a proper noun explicitly required by the setting.
```

For `uk`:

```text
Generate all player-facing game content in Ukrainian.
Keep technical ids in ASCII/kebab_case.
Do not translate ids.
Do not output English prose unless it is a proper noun explicitly required by the setting.
```

For `en`:

```text
Generate all player-facing game content in English.
Keep technical ids in ASCII/kebab_case.
Do not translate ids.
```

This must be unit-testable without calling an LLM.

### 6. Language diagnostics

Add a non-blocking diagnostic/warning for obviously wrong player-facing language.

Initial heuristic is enough:
- inspect titles, descriptions, dialogue lines, quest objectives/steps where available;
- if target language is ru/uk and player-facing text has a high ratio of English words/Latin prose, emit warning;
- do not flag technical ids;
- do not fail existing smoke scenarios unless the task explicitly adds a separate strict language gate.

Warning should be visible in validation/report paths if practical.

### 7. Product smoke scenario

Add scenario:

```text
content-language-policy
```

It should verify:
1. Russian policy can be created/selected.
2. Prompt/request instruction contains Russian language requirement.
3. Technical id policy remains ASCII/kebab_case.
4. A sample English player-facing text produces a warning under ru policy.
5. No real provider call is made.
6. Existing product smoke scenarios still pass.

### 8. State docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 009 accepted/complete and Slice 010 in progress/done according to actual work.

## Tests

Required tests:
1. policy default supports ru/uk/en.
2. prompt instruction provider emits correct ru instruction.
3. prompt/request path includes selected language instruction.
4. language diagnostics warn on obvious English prose under ru.
5. language diagnostics do not flag ASCII technical ids alone.
6. project policy save/load works if persistence implemented.
7. content-language-policy smoke passes.
8. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\\LLMGameCreator.Tests\\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ContentLanguage"
dotnet test tests\\LLMGameCreator.Tests\\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"

powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario generated-map-placement-preview
powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario content-language-policy

powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\\.devflow\\scripts\\check-all.ps1
```

## Manual verification

```text
1. Open LLM Artifacts page.
2. Confirm content language selector exists.
3. Select Russian.
4. Select full_small_rpg_seed or any preset.
5. Confirm generated request/prompt preview/status clearly says Russian content is required, if a prompt preview exists.
6. Generate artifacts using configured provider only if available.
7. Artifact Review should show Russian player-facing content for new generation.
8. Technical ids should remain ASCII/kebab_case.
```

If provider is unavailable, manual UI check may stop at prompt/request construction.

## Stop conditions

Stop and report if:
- generation path is too opaque to safely inject instruction;
- implementing persistence requires broad infrastructure rewrite;
- language detection requires new package dependency;
- `.sln` or `.csproj` changes are required;
- existing smoke scenarios would need fixture rewrites;
- more than 18 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with:
- files read;
- files changed;
- official docs added;
- content language policy behavior;
- persistence behavior;
- prompt/request integration point;
- language diagnostic heuristic;
- smoke scenario results;
- check-all/check-devflow results;
- remaining gaps;
- recommended next slice.
