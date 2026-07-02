# Goal 082 — Edit-Driven Unity Alpha StreamingAssets Handoff

This task pack is intended to be unpacked into the repository root and executed by Codex through `GOAL.md`.

Purpose: consume the real Goal 080 projected GamePackage and Goal 081 playthrough artifacts, produce a bounded Unity StreamingAssets handoff payload, add a small Unity probe source file that can read that payload, and prove the handoff through Application-side simulation/tests without bloating `AlphaRuntimeBootstrap.cs`.
