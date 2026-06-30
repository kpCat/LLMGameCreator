# External scouting — Goal 061 Full Campaign Playable Review Package RC

## Decision
Do not add external dependencies for Goal 061.

Goal 061 should reuse the repo-local Unity Alpha route, the existing Application-layer evidence pattern, existing JSON/hash validation utilities, and BCL-only package/review-package orchestration. The goal is not to integrate a new launcher framework, UI toolkit, installer builder, compression library, or packaging system.

## Relevant external references reviewed

- Unity Editor command-line / batchmode: useful for automated build/proof routes, but no new dependency is required.
- Unity standalone/player command-line arguments: useful for family/seed/package selection markers in the existing Alpha proof route.
- Unity StreamingAssets: already appropriate for deterministic review-package payloads and physical media/package files.
- Compression/installer tools such as SharpCompress, NuGet packaging tools, WiX, Squirrel, Velopack, Inno Setup: defer. They are unnecessary until the review package shape is stable and would add packaging policy noise now.
- UI frameworks / Unity UI Toolkit expansion: defer. Goal 061 should only add narrow marker/diagnostic/selector support if needed.

## Rationale
The repository already has a working Unity Alpha proof chain. Goal 061 should consolidate it into a runnable/reviewable package candidate and automated smoke matrix, not broaden dependencies. If a real Unity player build is produced, heavy build/log/cache outputs should remain ignored unless an existing repo policy explicitly tracks them. Commit compact manifests, hashes, scripts, reports and review-package metadata.
