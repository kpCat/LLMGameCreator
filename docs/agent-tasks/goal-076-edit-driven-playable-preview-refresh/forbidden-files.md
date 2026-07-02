# Forbidden files / areas

Do not change these unless GOAL.md explicitly narrows and allows a tiny exception:

- public GamePackage schema definitions
- Runtime and Runtime.Abstractions
- Infrastructure provider / LLM / RAG / media provider code
- Lua / Scripting
- generator-library
- .sln and .csproj
- broad shared UI infrastructure unrelated to CampaignAuthoringReviewWorkspace
- branch / merge / rebase / cherry-pick / reset / stash / clean / force-push operations

Do not add external dependencies.

Do not perform broad refactors for style.

Do not replace the Goal 075 edit loop with a new UI. Extend the existing CampaignAuthoringReviewWorkspace composition using separate UserControls.

Do not convert the workspace page into a god-form.

Do not mark Goal 076 GREEN with only report flags. Product smoke must prove behavior through real artifacts and must fail on tampered/missing staged handoff data.
