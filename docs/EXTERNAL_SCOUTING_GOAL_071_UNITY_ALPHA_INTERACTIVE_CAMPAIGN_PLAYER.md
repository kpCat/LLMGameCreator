# External scouting — Goal 071 Unity Alpha Interactive Campaign Player

## Decision

Do not add external dependencies. Use the existing repo-local Unity Alpha project and the already proven StreamingAssets/player-command/marker route.

## Unity APIs to use carefully

- `Application.streamingAssetsPath` is the right runtime path for files staged under `Assets/StreamingAssets` in built players.
- `Input.GetKeyDown` / `KeyCode` are sufficient for the narrow interactive proof. Do not add the new Input System package.
- Unity IMGUI / `OnGUI` is acceptable for a temporary Alpha/debug review HUD. Do not introduce UI Toolkit, uGUI prefabs, TextMeshPro, Dear ImGui, or other UI dependencies.

## Rejected dependencies for this goal

- No UI Toolkit package or prefab authoring.
- No TextMeshPro.
- No Dear ImGui / custom runtime UI package.
- No ECS / gameplay framework.
- No new NuGet packages.

## Architectural position

Goal 071 should turn the existing marker/proof Unity Alpha into a thin interactive review player:

- select family/seed/package row;
- show current generated state and timeline step;
- advance actions through keyboard or command plan;
- emit deterministic logs proving state transitions;
- keep everything bounded and review-focused.

This is not the final game UI. It is a testable Alpha review surface.
