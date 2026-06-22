# Unity Archive Provider Job Plan Spec v1

A request says what is needed. A slot says where a future produced/imported artifact should live. A provider job says which future pipeline should fulfill it.

Provider job kinds:
- manual_import
- comfyui_future
- suno_future
- local_audio_future
- procedural_future
- none

Example expected output paths:
- assets/generated/portrait/portrait.npc.npc-alpha.png
- assets/generated/icon/icon.item.item-key.png
- audio/generated/ui_sfx/sfx.ui.click.wav
- audio/generated/music/music.theme.short_sfx.wav
- lua/generated/lua-request.inventory.lua

These are expected future outputs only. This slice must not write the generated output files.
