# Product Slice 003: Artifact Review -> Apply -> Package Assembly

## Goal

Move from “LLM generated valid artifacts” to “LLMGameCreator can accept artifacts and assemble a draft package”.

M4.1 proved strict generation works. Product Slice 001/002 improved capability selection. The next real value is:

```text
generate artifacts
-> review artifacts
-> approve artifacts
-> apply approved artifacts
-> package state exists
-> validate package
-> save/export package
```

## Baseline contracts

This slice should support only the known baseline contracts:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

Do not expand into all future contracts.

## Expected user flow

```text
1. Capability Picker: build/save selection.
2. LLM Artifacts: load selection, generate baseline contracts, stage for review.
3. Artifact Review: view staged artifacts.
4. Artifact Review: approve selected/all valid baseline artifacts.
5. Artifact Review or Package Export: apply approved baseline artifacts to draft package.
6. Validate package.
7. Save/export package output.
```

## Package assembly rules

### game_profile_v1

Map to package-level metadata or nearest existing equivalent:

```text
title
description
genre
tone
presentation mode
world topology
actor model
combat model
core loop
pillars
source context
```

### scene_pack_v1

Map to scene/location structures if they exist. If exact structures do not exist, preserve scenes in a package extension/generated-content section rather than inventing a broad runtime schema.

### quest_pack_v1

Map to quest definitions if they exist. If exact structures do not exist, preserve quests in a package extension/generated-content section.

### mechanics_pack_v1

Map to rule/mechanic definitions if they exist. If exact structures do not exist, preserve mechanics in a package extension/generated-content section.

## Provenance

Every applied artifact should preserve provenance:

```text
artifact_id
contract_id
capability_selection_id
generated_at/audit id if available
applied_at
source hash or content hash
```

## Validation

Package validation should confirm at least:

```text
package has title/id or equivalent
applied baseline artifact ids are unique
applied baseline contract ids are known
required baseline artifacts are present when applying full baseline set
basic scene/quest/mechanic ids are unique when available
raw preserved JSON remains valid JSON
```

Do not invent strict runtime rules that make current valid artifacts fail.

## UI requirements

Artifact Review should make it clear:

```text
valid artifact
invalid artifact
approved artifact
rejected artifact
requires approval
applied to package
```

There should be clear actions:

```text
Approve selected
Approve all valid
Apply approved to package
```

## Export/save

Use existing package/project save conventions if available.

If no suitable convention exists, write an inspectable draft output under the current game folder, for example:

```text
.llmgc/package-assembly/draft-package.json
.llmgc/package-assembly/package-assembly-report.md
```

Prefer existing project/package services over ad-hoc file writes.

## Non-goals

Do not implement runtime preview, live simulation, Lua execution, Unity export, or all future contract families.

## Done

This slice is done when:

```text
approved baseline artifacts can be applied
draft package assembly exists
package assembly can be saved/exported
package assembly validates
user can inspect result
check-all passes
```
