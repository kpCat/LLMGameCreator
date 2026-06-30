# External Scouting — Goal 056 Unity Alpha Media-Bound Playable Package

## Decision

Do not add new NuGet packages or Unity packages in Goal 056.

Goal 056 should use the existing repo-local Unity Alpha project and Unity APIs already available in the project. The goal is to prove that Goal 055 staged physical media can be consumed by a real media-bound playable/review package path, not to introduce a general media framework.

## Relevant references

- Unity StreamingAssets:
  - Files placed in `Assets/StreamingAssets` are copied verbatim to the built player.
  - Runtime code should use `Application.streamingAssetsPath` rather than hardcoded paths.
  - On Windows desktop this is file-system accessible; Android/WebGL have special URL/UnityWebRequest concerns and are out of scope for this Windows Alpha proof.
  - Reference: https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Application-streamingAssetsPath.html
  - Reference: https://docs.unity3d.com/6000.5/Documentation/Manual/StreamingAssets.html

- Unity JSON:
  - `JsonUtility` can serialize/deserialize JSON for simple DTOs, but it does not support arbitrary dictionaries well.
  - Goal 056 should prefer simple manifest DTO arrays/lists if Unity-side parsing is required.
  - Reference: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/JsonUtility.html

- Unity PNG loading:
  - `Texture2D.LoadImage` / `ImageConversion.LoadImage` can load PNG/JPG bytes into a texture.
  - Goal 056 should only load deterministic PNG fixtures already produced by Goal 054/055.
  - No image-generation providers are allowed.
  - Reference: https://docs.unity3d.com/ScriptReference/ImageConversion.LoadImage.html

- Unity PCM audio:
  - `AudioClip.Create` + `AudioClip.SetData` can create procedural audio clips from float samples.
  - Goal 056 may implement a narrow PCM-WAV parser for the deterministic BCL-generated fixture WAV format only.
  - No general audio codec framework is required.
  - Reference: https://docs.unity3d.com/6000.4/Documentation/ScriptReference/AudioClip.SetData.html

## Dependency options considered and rejected for Goal 056

### ImageSharp

Useful for .NET image manipulation, but license/commercial terms are not ideal for becoming a default project dependency, and Goal 054 already produced PNG fixtures BCL-only. Do not add.

### SkiaSharp

MIT and powerful, but adds native/graphics dependency surface. Not needed for fixture loading/proof. Do not add.

### NAudio

Useful for WAV/audio handling in .NET, but Goal 056 only needs a tiny deterministic PCM-WAV validation/loading proof. Do not add.

### Newtonsoft.Json

Could be useful in Unity, but the project should avoid new Unity package/dependency churn for this proof. If Unity-side JSON parsing is needed, use DTO-friendly `JsonUtility` or a tiny constrained parser for the manifest shape only. Do not add.

## Goal 056 direction

Use BCL/Application code to stage Goal 055 media package facts into a Unity Alpha StreamingAssets payload, then make the repo-local Unity Alpha runtime consume that payload narrowly:

- read manifest;
- validate file refs/hashes against staged files where practical;
- load PNG into a texture;
- parse deterministic PCM WAV fixture headers and create/load a clip proof;
- display/log media-bound family panel data;
- run automated diagnostic player smoke that proves the media-bound package was loaded.

If actual Unity CLI/build execution is not available locally, the task must not fake success. It must commit/push BLOCKED with exact reason and all completed Application evidence.
