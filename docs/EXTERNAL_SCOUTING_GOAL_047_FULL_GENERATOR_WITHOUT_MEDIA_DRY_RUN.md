# External Scouting — Goal 047 Full Generator Without Media Dry Run

## Decision

Do not add new dependencies for Goal 047.

The goal is not a generic workflow framework or CLI shell. It is a domain-specific full-generator dry-run and promotion proof over existing LLMGameCreator artifacts, package/runtime preview/export seams and multi-family generated template evidence.

## Reviewed options

### Stateless

Stateless is a mature .NET state-machine library and could model review/promotion states. It is useful as a future adapter if workflow complexity grows, but Goal 047 needs deterministic, serializable, evidence-friendly state records with causal diagnostics. A tiny in-house transition table is safer now.

Decision: defer.

### FluentResults

FluentResults provides a result-pattern library and MIT licensing, but LLMGameCreator already uses explicit diagnostic/evidence records and invalid/fake/leak matrices. Adding a dependency would not reduce the domain-specific work.

Decision: defer.

### OneOf

OneOf can model discriminated union-like returns. Goal 047 should avoid new generic abstractions and keep records explicit, JSON-friendly and stable for evidence.

Decision: defer.

### Spectre.Console / Spectre.Console.Cli

Spectre is useful for future CLI tooling, but Goal 047 must stay Application-layer and product-smoke driven. No command-line app or UI shell is required.

Decision: defer.

## Policy

Use BCL/System.Text.Json and existing repository patterns only.

No new NuGet packages.
No WinForms/UI.
No Runtime/Unity source changes.
No provider/LLM/RAG calls.
No media generation.
No public GamePackage schema changes.
