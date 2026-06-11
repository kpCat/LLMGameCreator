# WinForms Designer Rules

These rules are mandatory for every `Form` and `UserControl` in `LLMGameCreator.WinForms`.

## Required structure

Every visual class must be split into two files:

```text
SomeControl.cs
SomeControl.Designer.cs
```

`SomeControl.cs` contains:

- injected services;
- constructor overloads;
- event wiring;
- UI reaction logic;
- refresh/update methods.

`SomeControl.Designer.cs` contains:

- visual fields;
- `InitializeComponent()`;
- `Dispose(bool disposing)`;
- layout, docking, sizes, static text, columns and static child controls.

## Visual Studio Designer compatibility

Designer files must be conservative and CodeDOM-friendly:

- use block-scoped namespace syntax, not file-scoped namespace syntax;
- avoid target-typed `new()` in designer code;
- avoid collection expressions;
- avoid LINQ;
- avoid loops;
- avoid lambdas/event handlers;
- avoid injected services;
- avoid runtime data loading;
- avoid async code;
- prefer explicit arrays such as `new ColumnHeader[] { ... }`;
- prefer `this.` for fields and control calls.

## Constructors

Designable WinForms controls with injected services should expose a parameterless constructor for Visual Studio Designer:

```csharp
public SomePageControl()
{
    InitializeComponent();
}
```

The runtime constructor may accept services:

```csharp
public SomePageControl(IMyService service)
{
    _service = service;
    InitializeComponent();
    WireEvents();
}
```

The parameterless constructor must not start runtime operations, load files, call LLM, run validation or access DI.

## Event handlers

Event handlers should normally be wired in the runtime constructor or a dedicated `WireEvents()` method, not in `InitializeComponent()`.

## Business logic ban

Designer files must not contain business logic, data access, generation logic, runtime logic, validation logic or script execution.
