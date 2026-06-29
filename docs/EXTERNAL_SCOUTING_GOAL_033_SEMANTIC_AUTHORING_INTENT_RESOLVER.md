# Goal 033 external scouting — semantic authoring workspace and feature-driven intent resolver

## Decision

Do not add external dependencies for Goal 033.

Goal 033 needs a deterministic Application-layer authoring/intent planning seam, not a runtime scripting engine, UI framework, JSON Schema dependency, validation framework, or templating engine. The safest implementation is BCL-only typed C# records plus deterministic validators and evidence writers, consistent with Goals 030-032.

## Looked at / considered

### NJsonSchema

- Usefulness: JSON Schema reading/generation/validation and code generation could be useful later for user-facing import/export, schema documentation, or external tool integration.
- License: MIT.
- Decision: defer. Goal 033 should define our own UI-ready authoring schema records rather than introduce JSON Schema as the canonical contract.
- Reason: adopting JSON Schema now could make the editor contract look generic but would not solve domain semantics, feature provenance, applicability, inheritance, or semantic intent resolution.

### FluentValidation

- Usefulness: mature .NET validation library with a fluent rule API.
- License: Apache-2.0.
- Decision: defer.
- Reason: repository already uses deterministic diagnostic matrices and causal validators. Introducing a framework now would add a dependency and a different validation idiom without reducing the real semantic complexity.

### Scriban

- Usefulness: strong future candidate for programmatic dialogue/localization/template rendering because it is a safe/lightweight .NET templating engine.
- License: BSD-2-Clause.
- Decision: defer.
- Reason: Goal 033 must not render final dialogue prose. It should output dialogue/event/quest intent records and template hints only.

### DynamicData

- Usefulness: future UI/MVVM dynamic collection projection candidate, especially when WinForms/WPF dynamic editor pages arrive.
- License: MIT.
- Decision: defer.
- Reason: Goal 033 is not a UI goal. It should emit deterministic authoring workspace manifests that a later UI can consume.

## Architectural conclusion

Goal 033 should be BCL-only and should produce:

- dynamic authoring workspace manifests;
- lore intake skeleton records;
- manual/programmatic/LLM-candidate provenance separation;
- feature-driven content intent resolution records;
- deterministic evidence for frontier/gothic/caravan/metamodule scenarios;
- strict invalid/fake/leak diagnostics.

The LLM role remains explicitly limited: lore drafting and candidate suggestions only. The program owns feature applicability, inheritance, influence resolution, authoring schema, validation, and intent planning.
