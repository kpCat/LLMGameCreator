# Slice 020 Spec: Asset/Audio/Lua Request Pipeline

## Suggested services

```text
UnityArchiveAssetAudioLuaRequestService
UnityArchiveRequestMaterializationService
```

One service is enough if simpler.

## Suggested models

```text
UnityArchiveRequestPipelineRequest
UnityArchiveRequestPipelineResult

UnityArchiveAssetRequest
UnityArchiveAudioRequest
UnityArchiveLuaModuleRequest

UnityArchiveRequestSourceRef
UnityArchiveRequestDiagnostic
UnityArchiveRequestReadiness
UnityArchiveRequestProviderKind
UnityArchiveAssetKind
UnityArchiveAudioKind
UnityArchiveLuaModuleKind
```

## Inputs

```text
project root
GameDesignBrief
UnityTargetProfile
UnityGameArchiveManifest
runtime module contracts
optional GamePackageDefinition
optional generated-content indexes/payload result
```

## Outputs

```text
.llmgc/unity-archive/assets/asset-requests.json
.llmgc/unity-archive/assets/asset-request-index.json
.llmgc/unity-archive/audio/audio-requests.json
.llmgc/unity-archive/audio/audio-request-index.json
.llmgc/unity-archive/lua/module-requests.json
.llmgc/unity-archive/lua/modules-index.json
```

## Determinism

- stable request ids;
- stable sorting by category/kind/source/id;
- no timestamps;
- UTF-8 without BOM where possible;
- empty arrays are valid;
- repeated unchanged run must be byte-identical.

## Safety

- no path traversal;
- no absolute archive-relative paths;
- all output must stay under `.llmgc/unity-archive/`.

## Important constraints

Do not invent final assets. Only request them.

For example, do not create `portrait_guard_kaelen.png`; create metadata saying that such a portrait is needed.

Do not call ComfyUI/Suno/LLM. Provider execution belongs to later slices.
