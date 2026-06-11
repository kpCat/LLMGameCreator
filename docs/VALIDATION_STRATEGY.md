# Validation Strategy

Цель валидатора — защитить проект от AI-generated каши.

Каждый generated draft, Lua script, asset request, map, entity, quest, dialogue и GamePackage проходит validation.

## Severity

```text
Fatal    — пакет нельзя загрузить/исполнить.
Error    — пакет невалиден, apply/export нельзя.
Warning  — пакет работает, но есть риск.
Info     — диагностическая подсказка.
```

## Validation phases

```text
1. File system validation
2. JSON/schema validation
3. Id/reference validation
4. Feature/system validation
5. Lua manifest validation
6. Lua dry-run validation
7. Asset catalog/contract validation
8. Map/entity/component validation
9. Dialogue/quest/interaction validation
10. Runtime smoke simulation
11. Export/Unity compatibility validation
```

## Map validation

Для finite map:

- width/height valid;
- defaultTileId exists;
- tile overrides inside bounds;
- entity positions inside bounds;
- player start exists and walkable;
- blocked tiles respected;
- portals target valid map/position.

Для chunked map:

- chunkSize valid;
- generatorScriptId exists;
- generator script type = generator;
- dry-run generates valid chunk;
- generated tiles reference known tile prototypes;
- generated entities reference known prototypes.

## Runtime smoke simulation

Минимальный smoke:

```text
Load GamePackage
Validate
Start runtime
Execute Wait
Execute Move if map mode
Execute Interact if interactable nearby
Serialize state
Deserialize state
Continue runtime
```

## Draft validation

```text
raw output
  -> extract draft
  -> parse
  -> validate schema
  -> validate references
  -> validate generated Lua/assets
  -> preview
  -> user approve
  -> apply patch
  -> validate whole package
```

Если validation failed — draft не применяется.

## Validation report

Issue должен содержать:

```text
severity
code
message
path
relatedId
suggestedFix
sourceFile
line/entry if known
```

## Codex rule

Новая функциональность не считается законченной, пока не добавлены:

- validation rule;
- sample data;
- smoke test или validation test;
- docs update.
