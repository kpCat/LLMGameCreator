# External Scouting — Goal 034 Strict LLM Draft Artifact Loop

## Decision

Do **not** add external dependencies in Goal 034.

Goal 034 is not about making LLM powerful. It is about making LLM less dangerous and less central: the system must accept only contract-bound draft candidates, quarantine them, validate them, produce repair requests as data, and promote nothing unless deterministic validation and provenance rules allow it.

This is best implemented BCL-only in the Application layer first.

## Current repository context

- Goal 030 introduced semantic artifact contract registry and compatibility planning.
- Goal 031 introduced semantic pack composition blueprint evidence and remains produced-for-review.
- Goal 032 introduced dynamic semantic feature system and remains produced-for-review.
- Goal 033 introduced semantic authoring workspace and feature-driven intent resolver, and was accepted by user decision.
- The current recommended next work is `goal_034_strict_llm_draft_artifact_loop`.

## Scouted libraries / tools

### NJsonSchema

What it offers:
- JSON Schema read/generate/validate for .NET.
- Can generate schemas from .NET classes and validate JSON data.
- MIT-licensed source markers are visible in the repo.

Pros:
- Useful later for external JSON Schema interoperability.
- Mature .NET library.

Cons / risks for Goal 034:
- Pulls in extra dependencies and a general schema model before the project has stabilized its own domain contracts.
- General schema validation is not enough; this project needs causal diagnostics, provenance rules, promotion boundaries, repair slots and artifact-family-specific checks.
- Would not remove the need for a domain validator.

Decision:
- Defer.
- Keep Goal 034 BCL-only and write typed domain validators.
- Reconsider as an optional import/export adapter after the draft artifact contract stabilizes.

### JsonSchema.Net / json-everything

What it offers:
- System.Text.Json-oriented JSON Schema support and adjacent JSON tools.

Pros:
- Modern .NET JSON ecosystem.
- MIT metadata appears in project identification.
- Better System.Text.Json alignment than Newtonsoft-based stacks.

Cons / risks for Goal 034:
- Still a general JSON Schema dependency.
- We need deterministic artifact promotion semantics, not just schema acceptance.
- Adds surface area before the LLM draft envelope/provenance contract is validated by tests.

Decision:
- Defer as optional adapter.

### FluentValidation

What it offers:
- Strongly typed validation rules for .NET.
- Apache-2.0 license.

Pros:
- Good general .NET validation framework.
- Could reduce boilerplate later.

Cons / risks for Goal 034:
- Current repository already uses explicit causal diagnostic matrices.
- Fluent rules may hide domain causality unless carefully wrapped.
- Adds a dependency without reducing the hard part: provenance, quarantine, repair and promotion constraints.

Decision:
- Defer.

### Scriban

What it offers:
- Fast, safe, lightweight .NET text templating engine.

Pros:
- Good candidate for later programmatic text/template realization.
- Useful for future localization/dialogue template rendering.

Cons / risks for Goal 034:
- Goal 034 must not generate final prose or dialogue text.
- Adding templating now would blur the boundary between LLM draft candidates and deterministic content realization.

Decision:
- Defer to a future programmatic content realization / template rendering goal.

## Architectural conclusion

Goal 034 must implement a strict draft-artifact loop as domain code:

1. Draft request records derived from Goal 033 intents and accepted authoring inputs.
2. Quarantined candidate envelopes that can represent manual, imported or LLM-produced drafts.
3. Deterministic validation before promotion.
4. Structured repair request records as data, not live LLM calls.
5. Promotion only into approved draft artifacts / seed candidates, never final prose, runtime code, GamePackage schema, UI, Unity, Lua or provider paths.

## LLM-minimal policy

Allowed in Goal 034:
- Define request/envelope/validation/repair/promotion contracts for future LLM draft candidates.
- Use deterministic fixtures that simulate candidate JSON from LLM/manual/imported sources.
- Produce repair request artifacts as JSON records.

Forbidden in Goal 034:
- Calling any LLM/provider/RAG.
- Generating final dialogue prose, quest text, lore text or runtime content as accepted output.
- Treating LLM output as accepted because it is syntactically valid JSON.
- Promoting candidates without deterministic contract/provenance/semantic checks.
