# Goal 074 external scouting: schema-driven authoring/review workspace

## Decision
Do not add external UI dependencies for Goal 074.

The project already has a WinForms editor surface, so this goal should use existing WinForms patterns and UserControls rather than adding a new UI framework. The purpose is not visual polish; it is a schema-driven review/authoring workspace that proves the full campaign matrix can be inspected and adjusted through a real editor-facing contract.

## Deferred options
- Avalonia / WPF / MAUI: not used. Introducing a new UI stack would expand scope and risk.
- ReactiveUI / DynamicData / MVVM frameworks: deferred. Useful later only if the editor UI grows beyond simple WinForms controls.
- Third-party grid/visualization controls: not used. The goal must remain bounded and repository-local.

## Important architecture rule
Goal 074 should not hardcode one giant form for every gameplay system. It should create a dynamic workspace schema and bind WinForms controls to schema groups/fields/diagnostics. Every tab/sub-tab/panel added by this goal must be a separate UserControl, not a pile of code in a main form.
