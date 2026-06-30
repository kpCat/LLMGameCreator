# External scouting — Goal 053 Media Asset Campaign Orchestration

## Purpose

Goal 053 moves the generator beyond the `full_generator_without_media_verification` dry run into a media-campaign orchestration layer. It must not call image/audio providers. It must define deterministic request queues, license/provenance rules, review/promotion decisions, binding manifests and fixture media outputs so future ComfyUI/Fooocus/Freesound/OpenGameArt/provider adapters can be added without contaminating the core generator.

## Current external options reviewed

### ComfyUI

- Status: candidate future local image-generation adapter.
- License/risk: ComfyUI is GPL-3.0. Direct code integration or bundling can create copyleft/distribution concerns. Future integration should be out-of-process, optional, user-installed and adapter-bound.
- Use now: no.
- Use later: yes, as an optional local process/HTTP workflow adapter. Store workflow JSON references and model/license metadata, not ComfyUI code.

### Fooocus

- Status: candidate future simplified local image-generation adapter.
- License/risk: Fooocus is GPL-3.0. Model licenses still matter independently. Reported default/model licensing can be more restrictive than the tool license.
- Use now: no.
- Use later: only through optional user-installed external-process adapter with explicit model-license capture.

### Stability / Stable Diffusion model/API ecosystem

- Status: future provider/model family, not a current dependency.
- License/risk: terms/model licenses can change by model/provider and must be captured per run. Do not hardcode assumptions that output is always commercially safe.
- Use now: no.
- Use later: adapter can create requests and record provider/model/license policy, but core generator must work without it.

### OpenGameArt

- Status: useful source for seed/placeholder/open assets.
- License/risk: assets have mixed licenses. CC0 is simplest. CC-BY needs attribution. CC-BY-SA/GPL-like assets can create share-alike or redistribution obligations. Each asset must carry license/provenance.
- Use now: no network/import; model the license/provenance ledger.
- Use later: import adapter should quarantine candidates until license policy accepts them.

### Freesound / Pixabay / external SFX libraries

- Status: useful future audio source adapters.
- License/risk: Freesound assets are Creative Commons with per-asset requirements. Pixabay-like sites advertise royalty-free/no-attribution assets but terms must be captured at import time. No-license/no-provenance means reject.
- Use now: no network/import; create audio request/provenance contracts.
- Use later: optional import adapter with attribution ledger and audit proof.

### Audacity / audio processing tools

- Status: possible manual editing tool, not generator dependency.
- License/risk: GPL; do not bundle or integrate code.
- Use now/later: external manual tool only.

## Decision for Goal 053

Do not add external dependencies.
Do not call media providers.
Do not generate AI images/audio.
Do not import internet assets.

Implement BCL-only Application-layer contracts and deterministic fixture media proof:

- media request campaign;
- style/profile/family media slots;
- license/provenance policy;
- media candidate quarantine;
- review/promotion decisions;
- deterministic fixture image/audio placeholder payloads;
- binding manifest from generated semantic/runtime/family ids to media ids;
- export/preview payload proof;
- invalid/fake/leak matrix.

Future adapters should plug in after this goal as optional modules:

- ComfyUI workflow adapter;
- Fooocus adapter;
- Stable Diffusion/API adapter;
- OpenGameArt import adapter;
- Freesound/Pixabay audio import adapter;
- manual file import adapter.

