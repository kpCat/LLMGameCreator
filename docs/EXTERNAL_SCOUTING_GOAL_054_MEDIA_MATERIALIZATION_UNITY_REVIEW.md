# External scouting — Goal 054 Media Materialization And Media-Bound Review Package Smoke

## Decision

Do not add new external dependencies for Goal 054.

Goal 053 already proved media asset campaign governance with fixture assets. Goal 054 must make those assets more concrete by materializing deterministic media files and binding them into preview/export/review payloads. This should be done BCL-only first.

## Candidates considered

### SixLabors ImageSharp

ImageSharp is powerful for image generation and transformation, but current licensing is not a simple universal permissive dependency. Six Labors documents commercial licensing requirements for some closed-source/commercial usage thresholds. This makes it a poor default dependency for the generator core at this stage.

Decision: defer. Do not add.

### SkiaSharp

SkiaSharp is MIT-licensed and useful for future image rendering, atlas generation, text drawing, icon compositing, and possible editor tooling. However it brings native/runtime packaging and a wider graphics surface. Goal 054 only needs deterministic placeholder media files and proof bindings.

Decision: defer as optional future adapter. Do not add.

### BCL-only PNG/WAV materialization

PNG can be produced with a small deterministic writer using PNG chunks, zlib/deflate stream semantics, CRC32 and simple pixel buffers. WAV PCM files can be produced with RIFF/WAVE headers and deterministic sample data. This is enough for media fixture proof without external packages.

Decision: use BCL-only deterministic fixture materializers. No provider, no network, no external media imports.

## Goal 054 boundary

Goal 054 may create deterministic fixture images/audio from already-governed Goal 053 bindings. It must not call ComfyUI/Fooocus/Stability/Freesound/OpenGameArt/Pixabay or any network/provider. It must not import arbitrary user media. It must not change the public GamePackage schema.

If a narrow Unity Alpha review-package smoke can be done by consuming already existing Unity/export seams without risky broad changes, it is allowed as a bounded optional proof. If Unity changes become broad or require editor execution that is not already part of the local flow, stop and commit BLOCKED rather than pretending success.
