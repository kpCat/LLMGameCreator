# LLMGameCreator Product Slice 008 Pack

## Purpose

Product Slice 008 has two connected goals.

First, repair the product seam found during manual testing:

```text
Artifact Review / Save decisions + apply
-> generated package is written to .llmgc/package-assembly/package.json
-> Runtime Preview still starts the old active root package
```

Second, add the first safe quest/dialogue preview stubs:

```text
select NPC / dialogue / quest
-> preview dialogue lines
-> start quest preview
-> mark next quest step preview
-> no real effects execution
```

This is still not Unity and not full gameplay simulation.

## Apply

Unzip this archive at repository root.

Then give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/008_CODEX_PROMPT.md
```

## Recommended Codex reasoning level

Use Codex reasoning: **High**.

Reason:
This slice includes a critical current-package activation seam plus in-memory quest/dialogue preview state and Runtime Preview UI. Medium may miss the project package/current package boundary. Max/Ultra is not needed unless High fails.

## Archive note

This pack intentionally places README/manifest under `docs/agent-tasks/NEXT_PRODUCT_SLICE/` and does not add root README files.
