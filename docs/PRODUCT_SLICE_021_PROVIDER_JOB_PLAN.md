# Product Slice 021: Unity Archive Provider Job Plan v1

Goal: turn Slice 020 asset/audio/Lua request metadata into deterministic provider-specific job plans and fulfillment slots.

Main archive outputs:
- production/fulfillment-plan.json
- production/readiness-report.json
- assets/asset-slots.json
- audio/audio-slots.json
- lua/module-slots.json
- providers/manual-import/jobs.json
- providers/comfyui/jobs.json
- providers/suno/jobs.json
- providers/local-audio/jobs.json
- providers/procedural/jobs.json

Non-goals: no provider execution, no ComfyUI/Suno integration, no generated image/audio/Lua files, no Unity project, no Runtime/GamePackage schema/WinForms changes.
