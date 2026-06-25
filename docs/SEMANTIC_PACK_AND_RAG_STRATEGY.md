# Semantic Pack And RAG Strategy

Status: proposed strategic documentation after Goal 003.

## Purpose

Semantic packs give the generator shared meaning. They are not just dictionaries. They are controlled game-generation vocabulary.

A semantic pack lets generators understand terms such as:

- biome/swamp;
- tone/mysterious;
- faction_relation/hostile;
- item_affordance/quest_item;
- entity_role/vendor;
- quest_motif/recover_cache;
- dialogue_intent/threaten;
- location_mood/dangerous.

The point is not to store every possible word. The point is to give procedural systems enough meaning to combine content safely and coherently.

## Semantic Pack Layers

Recommended layers:

1. Core semantic pack
   - small, curated, shipped with the combiner;
   - stable terms needed by generators;
   - should remain compact.

2. Genre semantic pack
   - fantasy, realism, zombie survival, colony sim, city builder, adult drama, post-apocalypse, etc.;
   - adds genre-specific terms and relations.

3. Project semantic pack
   - terms specific to one generated game;
   - factions, biomes, resources, status effects, cultures, laws, taboos, professions, locations.

4. Imported semantic pack
   - optional adapter output from WordNet, ConceptNet, Wikidata, OSM-like data, custom CSV/JSON, or user's notes;
   - never trusted blindly;
   - normalized, filtered, validated, and reviewed before use.

5. LLM-proposed semantic candidates
   - LLM may suggest new terms;
   - candidates remain untrusted until accepted or rejected.

## RAG Role

RAG can help author semantic packs, but runtime should not depend on RAG.

Recommended flow:

```text
User notes / existing packs / external datasets
-> local RAG index for authoring
-> LLM suggests terms and relations
-> semantic validator checks ids, relations, duplicates, conflicts
-> user or policy accepts candidates
-> compiled semantic catalog
-> deterministic generators consume the catalog
```

Runtime flow:

```text
compiled semantic catalog + seed + rule packs
-> deterministic generation
-> generated package
-> runtime
```

No runtime LLM/RAG is required.

## What A Semantic Term Should Contain

Minimum useful fields:

- id;
- kind;
- display name;
- status: known, candidate, deprecated, conflict, invalid;
- optional aliases;
- optional tags;
- optional relations;
- optional generation hints;
- optional constraints;
- source/provenance.

## Relations

Relations should be game-useful, not merely linguistic.

Useful examples:

- requires;
- excludes;
- implies;
- compatible_with;
- stronger_than;
- weaker_than;
- belongs_to_biome;
- produced_by;
- consumed_by;
- traded_by;
- hostile_to;
- allied_with;
- unlocks;
- causes;
- cures;
- resists;
- forbidden_in_tone;
- preferred_in_tone.

## What Not To Do

Do not import huge external datasets directly into generation.

Bad approach:

```text
Download massive knowledge graph
-> dump into semantic pack
-> expect better games
```

This increases noise and can make generation worse.

Better approach:

```text
External source
-> adapter
-> game-relevant subset
-> normalized candidates
-> validation
-> approval
-> compact semantic pack
```

## Example For Quest Generation

Semantic input:

- quest_motif/recover_cache;
- location_mood/dangerous;
- item_affordance/quest_item;
- entity_role/rival;
- faction_relation/hostile.

Generator can produce:

- recover item from dangerous region;
- rival NPC is involved;
- reward/progress rule applies;
- dialogue intent can be warning, threat, bargain, or confession.

The LLM is not needed to invent each quest instance.

## Semantic Pack Success Criteria

Semantic packs are working when:

- new project-specific terms can be added without C#;
- generated content uses terms coherently;
- invalid/conflicting terms are caught before package generation;
- LLM suggestions are reviewed as candidates;
- deterministic generators can produce varied content from the same compact semantic base;
- runtime receives compiled data, not raw LLM output.

## Future Work

Important future goals:

- semantic candidate review workflow;
- semantic import adapters;
- semantic relation validator;
- semantic-pack diff/comparison;
- semantic-guided quest/dialogue/item/biome generation;
- local RAG authoring helper;
- compiled semantic catalog export.
