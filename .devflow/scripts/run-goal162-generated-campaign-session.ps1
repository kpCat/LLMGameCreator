[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter 'FullyQualifiedName~Goal162'
