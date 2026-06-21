# Product Slice 009: Generated NPC/Encounter Map Placement

## Goal

Make generated NPCs and encounters visible on the Runtime Preview map.

## Desired user flow

```text
1. Generate/apply full_small_rpg_seed.
2. Use assembled package as current.
3. Runtime Preview -> Start.
4. Map shows player marker plus generated NPC/encounter markers.
5. Select NPC marker or generated browser entry.
6. Details/log show NPC name, scene/region refs, linked dialogue refs.
7. Move near NPC/encounter marker.
8. Inspect/interact nearby generated marker.
9. Log shows generated NPC/encounter preview text.
```

## Placement rules

Placement must be deterministic and safe:
- resolve NPC/encounter SceneId -> generated scene SourceId -> PackageMapId;
- if scene missing, try region linked scenes;
- if still unresolved, use current/start map and record warning/detail;
- place markers on map bounds;
- avoid player current/start position if possible;
- avoid duplicate marker positions if possible;
- prefer passable tiles if map passability is available.

## Marker types

Minimum:
```text
npc
encounter
```

Optional:
```text
item
quest
dialogue
```

Keep this slice focused on NPCs and encounters.

## Runtime behavior

This should be a Runtime Preview overlay, not a runtime engine rewrite.

Do not mutate package schema.
Do not rewrite `DefaultGameRuntime`.
Do not execute generated effects.
