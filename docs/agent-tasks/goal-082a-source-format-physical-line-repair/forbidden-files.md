# Forbidden files / areas

Do not change:

```text
public GamePackage schema
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.Infrastructure/** provider / LLM / RAG / media provider code
src/LLMGameCreator.Scripting/**
generator-library/**
*.sln
*.csproj
packages.lock.json / lock files
Unity scenes, prefabs, project settings, build settings
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
binary media assets
```

Do not:

```text
branch
merge
rebase
cherry-pick
reset
stash
clean
force-push
rewrite history
remove the user’s `adult docs` commit or documentation
add external dependencies
add provider integrations
call external providers
add real NSFW image/media fixtures
mark Goal 082 accepted/passed
mark adult docs as an active implementation milestone
```

Unity scope is limited to reformatting and guarding the existing independent probe script `EditDrivenGamePackageHandoffProbe.cs`. Do not wire it into `AlphaRuntimeBootstrap.cs` in this hotfix.
