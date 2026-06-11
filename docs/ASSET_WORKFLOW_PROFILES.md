# Asset Workflow Profiles

Workflow profile описывает, как редактор должен запускать внешний генератор ассетов.

## Цели

- не хардкодить внутренности ComfyUI workflow в коде;
- переиспользовать заранее подготовленные workflows;
- запускать batch generation;
- валидировать output по asset contract.

## Типы workflow profiles

- `tileset_biome_32px`;
- `character_spritesheet_4dir_4frames`;
- `item_icon_64px`;
- `ability_icon_64px`;
- `npc_portrait_expression_set`;
- `dialogue_background`;
- `scene_image`;
- `sfx_short_0_5_3s`;
- `music_loop_short`;
- `ambient_loop`.

## Пример ComfyUI profile

```json
{
  "id": "workflow/comfy/portrait_expression_set_v1",
  "provider": "comfyui",
  "assetType": "portrait_expression_set",
  "targetContractId": "contract/portrait_set/basic_expressions",
  "workflowFile": "workflows/comfyui/portrait_expression_set_v1.json",
  "endpointProfileId": "comfy/local-main",
  "parameters": {
    "positivePrompt": "nodes.6.inputs.text",
    "negativePrompt": "nodes.7.inputs.text",
    "seed": "nodes.3.inputs.seed",
    "outputPrefix": "nodes.12.inputs.filename_prefix"
  }
}
```

## Важное правило

Workflow profiles можно готовить отдельно вместе с ChatGPT, а локальная LLM потом только заполняет prompts/parameters и запускает jobs.
