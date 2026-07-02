# Forbidden files / areas

Do not change:

- public GamePackage schema or src/LLMGameCreator.GamePackage/**
- src/LLMGameCreator.Domain/** unless only reading
- src/LLMGameCreator.Runtime/**
- src/LLMGameCreator.Runtime.Abstractions/**
- unity/**, including AlphaRuntimeBootstrap.cs
- src/LLMGameCreator.Infrastructure/** provider/LLM/RAG/media provider code
- src/LLMGameCreator.Scripting/** and Lua/Scripting execution paths
- generator-library/**
- samples/**
- templates/**
- .sln files
- .csproj files
- external dependency/package files

Do not add external dependencies.
Do not perform broad refactors.
Do not do branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.
Do not mark Goal 077 accepted/passed.
Do not mark Goal 072 accepted or resolved.
Do not rewrite or remove Goal 075/076 evidence except reading it as source input.
