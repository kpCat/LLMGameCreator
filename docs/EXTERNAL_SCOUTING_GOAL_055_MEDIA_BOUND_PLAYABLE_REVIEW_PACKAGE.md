# External scouting — Goal 055 Media-Bound Playable Review Package Smoke

## Decision

Do not add external dependencies for Goal 055.

Goal 055 should prove that Goal 054 physical media fixtures can be staged into a media-bound playable/review package and consumed by the existing repo-local Unity Alpha path or a bounded Unity-compatible loader/proof seam. The goal must not call real media providers, network importers, LLM/RAG/provider paths, Lua, or mutate public GamePackage schema.

## Useful current facts

- Unity `Application.streamingAssetsPath` is the correct runtime access point for files staged in `StreamingAssets`; on desktop platforms this is a regular directory, while Android/WebGL require URL/UnityWebRequest handling. Goal 055 targets repo-local Windows/desktop review smoke only and must explicitly avoid Android/WebGL assumptions.
- Unity `ImageConversion.LoadImage` can load PNG/JPG/EXR bytes into a `Texture2D`; PNG is loaded as ARGB32. This is sufficient for fixture PNG binding proof without adding ImageSharp/SkiaSharp.
- WAV can be verified and optionally converted into a Unity `AudioClip` through a narrow PCM WAV parser in repo-local Unity code if needed. Do not add NAudio or other audio packages in Goal 055.

## Deferred dependencies / adapters

- ImageSharp: useful image library, but current licensing/commercial terms make it a bad default dependency for this repo at this stage.
- SkiaSharp: MIT and useful later, but native/graphics dependency surface is unnecessary for deterministic fixture PNGs.
- NAudio: useful for audio tooling, but not needed for small deterministic WAV fixture validation and could complicate Unity packaging.
- ComfyUI/Fooocus/Stability/Freesound/OpenGameArt: future optional providers/importers only. Goal 055 must not call them.

## Goal 055 dependency policy

- No new NuGet packages.
- No provider/network calls.
- No real media generation.
- Use existing Goal 054 physical PNG/WAV/bundle fixtures.
- Prefer BCL + existing Unity APIs where Unity source integration is required.
