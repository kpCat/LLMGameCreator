# Semantic Packs

These compact packs are deterministic authoring inputs for `semantic_pack_contract_v1`.

Add project-specific meaning by creating a `project/*` layer with safe ids, `known` terms and game-useful relations such as `prefers_quest_pattern`, `prefers_dialogue_intent` or `prefers_interaction_family`. Candidate imports belong in `imported_candidate/*` or `llm_candidate/*` and stay quarantined until the data status and layer kind are explicitly changed.

No LLM, RAG index, provider, Lua, Unity or media execution is required to compile these packs.
