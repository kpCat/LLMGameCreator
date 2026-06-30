# External Scouting — Goal 057 Unity Alpha Multi-Family Playable Loop

## Decision

No new third-party dependency is added in Goal 057.

Goal 057 should consume existing repository-local Unity Alpha infrastructure and previously generated compact evidence:
- Goal 043 multi-family simulatable loops;
- Goal 047 full generator without media dry-run;
- Goal 055 media-bound playable review package;
- Goal 056 Unity Alpha media-bound playable package.

The goal may narrowly extend the existing repo-local Unity Alpha runtime/bootstrap code, but it must not introduce a new Unity package, a new rendering framework, a new input system dependency, a provider/media generator, or a runtime LLM/RAG path.

## Unity references considered

- Unity `Application.streamingAssetsPath` is the right runtime access point for files staged under `Assets/StreamingAssets`.
- Unity `ImageConversion.LoadImage` can load PNG/JPG bytes into a `Texture2D`; Goal 056 already proved PNG media load markers, so Goal 057 can reuse or extend that bounded route.
- Unity Player command-line arguments support automated standalone-player execution, which fits the existing Alpha diagnostic/play-loop route.
- Unity Test Framework command-line docs confirm `-batchmode` automation pattern for Unity-side tests, but Goal 057 should prefer the existing project-specific Alpha build/player diagnostic route if it already exists.

## Libraries deferred

- ImageSharp: not used. It introduces licensing/commercial-policy considerations and is unnecessary for tiny deterministic media fixtures.
- SkiaSharp: not used. Useful later for richer image generation/composition, but it introduces native/platform packaging concerns.
- NAudio: not used. WAV fixture validation/loading is already narrow enough for BCL/Unity-side proof.
- Unity UI Toolkit/TextMeshPro packages: not added. If the existing Unity Alpha already uses IMGUI, Goal 057 should continue with the narrowest existing presentation route.

## Architectural conclusion

Goal 057 should be a real playable/smoke step, not another registry:
- stage accepted Goal 056 media into Unity Alpha;
- map three generated family loops to visible/automated Unity family modes;
- run deterministic player traces for all families;
- prove media + family loop markers;
- write compact review-package evidence.

No LLM, provider, network, external media generation, broad Runtime/GamePackage schema mutation, or new Unity package is allowed.
