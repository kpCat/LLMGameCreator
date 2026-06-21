# Product Slice 010: Official Product Plan + Content Language Policy Foundation

## Goal

Create a persistent official product plan in the repository and add the first usable content language policy foundation.

This slice exists because the project has grown from a single generated RPG prototype into a modular Game Assembly Workbench. The repository needs a stable direction document, and generated content should stop defaulting to English when the intended game language is Russian/Ukrainian.

## Scope

1. Add official product plan docs.
2. Add content language policy foundation.
3. Make LLM artifact generation prompts include the selected content language instruction.
4. Keep technical ids ASCII/kebab_case.
5. Add non-blocking language diagnostics/warnings for obviously wrong player-facing language.
6. Add smoke coverage.

## Non-goals

Do not implement:
- semantic world model;
- procedural quest engine;
- imported map pipeline;
- lazy world generation;
- offscreen simulation;
- translation engine;
- runtime text localization;
- UI localization overhaul.

## Content language policy

Minimum languages:

```text
ru
uk
en
```

Rules:
- UI language and game content language are separate concepts.
- Technical identifiers stay ASCII/kebab_case.
- Player-facing titles/descriptions/dialogue/objectives should obey content language.
- Existing old generated content is not auto-translated.

## Desired prompt instruction for ru

```text
Generate all player-facing game content in Russian.
Keep technical ids in ASCII/kebab_case.
Do not translate ids.
Do not output English prose unless it is a proper noun explicitly required by the setting.
```

## Desired prompt instruction for uk

```text
Generate all player-facing game content in Ukrainian.
Keep technical ids in ASCII/kebab_case.
Do not translate ids.
Do not output English prose unless it is a proper noun explicitly required by the setting.
```

## Desired prompt instruction for en

```text
Generate all player-facing game content in English.
Keep technical ids in ASCII/kebab_case.
Do not translate ids.
```

## Done

Done when:
- official plan docs exist in repo;
- language policy has tests;
- LLM Artifacts UI/service path can select/store a content language;
- prompt/request builder includes the language instruction;
- language diagnostics can warn on obvious English player-facing text when target is ru/uk;
- existing product smokes still pass;
- a new content-language-policy smoke passes.
