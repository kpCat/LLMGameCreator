# LLMGameCreator Product Slice 004 Pack

## Purpose

Product Slice 004 reduces manual checking by adding a headless product smoke runner for the baseline flow.

Main goal:

```text
fixture approved artifacts
-> package assembly service
-> exported package.json
-> validation/assertions
-> smoke report
```

This automates the flow that was just manually verified through UI:

```text
LLM Artifacts
-> Artifact Review
-> Approve
-> Apply approved to package
-> package.json
```

## Apply

Unzip at repository root.

Then give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/004_CODEX_PROMPT.md
```

## Recommended Codex reasoning level

Use Codex reasoning: **High**.

Reason:
This task touches tests, fixtures, optional devflow script, package assembly behavior, and current state docs. It is not hard enough for Max/Ultra, but Medium can miss the important integration seams.

## Not included

This slice does not implement:

- new runtime preview;
- Unity runtime;
- Lua executor;
- new LLM generation;
- economy/balance simulation;
- broad package schema rewrite.
