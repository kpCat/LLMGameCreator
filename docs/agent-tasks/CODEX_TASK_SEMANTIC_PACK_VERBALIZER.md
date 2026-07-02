# Codex Task — Semantic Pack Verbalizer

## Task ID

`semantic-pack-verbalizer-v1`

## Goal

Add a deterministic template-based verbalizer for semantic/visual packs.

This is not an LLM. It should generate short explanations, debug text, diagnostic messages and limited phrase/name outputs from known tags and templates.

## Read first

- `docs/proposals/SEMANTIC_PACK_VERBALIZER.md`
- `docs/context/ADAPTIVE_GENERATOR_CONTEXT_BRIEF.md`
- Existing semantic catalog/pack docs and models if present.

## Allowed scope

Prefer:

```text
src/LLMGameCreator.Application/Design/SemanticPackVerbalization/**
tests/LLMGameCreator.Tests/Application/SemanticPackVerbalization/**
docs/candidates/semantic-pack-verbalization/**
```

## Forbidden scope

Do not:

- call LLM/providers;
- add free-form generation;
- create large phrase banks;
- modify runtime/player;
- change GamePackage schema;
- create UI.

## Required models

- `SemanticVerbalizationRequest`
- `SemanticVerbalizationContext`
- `VerbalizationTemplate`
- `PhraseBank`
- `NamePattern`
- `VerbalizationResult`
- `SemanticPackVerbalizer`
- `VerbalizationDiagnostic`

## Required behavior

- explain VisualRuleStack in short debug text;
- generate short description for BuildingVisualRecipe-like input;
- generate diagnostic explanation text;
- generate deterministic simple names from patterns;
- use phrase banks with seed selection;
- return diagnostics for missing templates/tags.

## Tests

- same input/seed gives same output;
- missing template produces diagnostic;
- necropolis living-human dwelling explanation mentions moderate domain influence;
- forbidden motif diagnostic is rendered clearly;
- phrase bank selection is deterministic;
- no LLM/provider call path exists.

## Stop conditions

Stop if implementation requires LLM or external generator.

## Final report

Report models, templates, phrase banks, tests and limitations.
