# Generator Plan Example Template Catalog

Status: v1 built-in starter catalog

## Purpose

The built-in example template catalog gives the Package Export page ready-to-use `.example.json` seeds. It removes the need to hand-write a generator plan example before running the existing one-click package export flow.

The catalog is deterministic and local. It does not call an LLM, provider/model, Lua, Unity, runtime preview, graphics, or sound generation.

## Application Surface

The catalog and materialization service live in:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanExampleTemplateModels.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanExampleTemplateCatalog.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanExampleTemplateService.cs
```

The service exposes:

```text
ListTemplates()
GetTemplate(id)
MaterializeAsync(request)
```

`MaterializeAsync` creates the target directory when needed and writes the selected template as a stable `.example.json` file. With `Overwrite=false`, an existing file is preserved and the result returns `Ok=false`.

## Built-in Templates

The v1 catalog includes:

```text
Sky Lantern Outpost
Clockwork Orchard
Storm Glass Lighthouse
Moss Courier Trail
Underroot Signal
```

Each template targets:

```text
game_profile_v1
scene_pack_v1
entity_pack_v1
quest_pack_v1
mechanics_pack_v1
semantic_pack_v1
```

Each template contains one ordered step for every target artifact. The exported package is assembled through the existing preview, approval/staging, approved artifact set, GamePackage assembly, and package export services.

## Materialized Files

The Package Export UI writes template files to:

```text
<CurrentGameFolder>/.llmgc/example-templates
```

If no current game folder is loaded, it writes to:

```text
%LOCALAPPDATA%/LLMGameCreator/example-templates
```

Manual `.example.json` selection remains supported. The template catalog only fills the source example path; it does not replace the manual path picker.

## Export Notes

`semantic_pack_v1` is acknowledged by the deterministic artifact producer, but it still has no GamePackage v1 field. Exports that include semantic artifacts can therefore finish as:

```text
succeeded_with_warnings
```

Review the diagnostics grid and markdown report before using the exported package.

## Adding A Template

To add a safe built-in template:

1. Add a new seed in `GeneratorPlanExampleTemplateCatalog`.
2. Use a stable lowercase id and `<id>.example.json` filename.
3. Keep the default six target artifacts unless a later contract expands the export flow.
4. Keep JSON deterministic and compatible with `GeneratorPlanPreviewService`.
5. Add or update a focused test proving preview and one-click export still work.
